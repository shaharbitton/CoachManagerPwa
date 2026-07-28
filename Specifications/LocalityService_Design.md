# עיצוב שירות יישובים — Locality Service Design

**תאריך:** יולי 2026  
**סטטוס:** עיצוב בלבד — טרם פותח

---

## 📡 מקור נתונים — CKAN Datastore

### Endpoint

```
GET https://data.gov.il/api/3/action/datastore_search?resource_id=d4901968-dad3-4845-a9b0-a57d027f11ab&limit=2000
```

### שדות רלוונטיים מהתוצאה

| שדה ב-API | שם בעברית | שימוש |
|---|---|---|
| `שם_ישוב` | שם יישוב | בחירת עיר ללקוח |
| `שם_מחוז` | שם מחוז | הגדרת אזור עבודה למאמן |
| `שם_נפה` | שם נפה | הגדרת אזור עבודה למאמן (רמת ביניים) |

### מבנה הנתונים (לאחר עיבוד)

```csharp
public record Locality(string Name, string District, string SubDistrict);
```

היררכיה:
```
מחוז (District)
  └── נפה (SubDistrict)
        └── יישוב (Locality)
```

דוגמה:
```
מחוז תל אביב
  └── נפת תל אביב
        ├── תל אביב - יפו
        ├── רמת גן
        ├── גבעתיים
        └── ...
```

---

## 🏗️ ארכיטקטורה

### 1. `LocalityService` — שירות מרכזי

```
Services/LocalityService.cs
```

**אחריות:**
- טעינת רשימת יישובים מ-API (פעם אחת per session)
- Cache ב-memory
- Fallback מקובץ `wwwroot/data/localities.json`
- חשיפת מתודות חיפוש

**ממשק:**

```csharp
public interface ILocalityService
{
    Task InitializeAsync();
    
    // יישובים
    Task<IReadOnlyList<Locality>> GetAllAsync();
    Task<IReadOnlyList<string>> SearchLocalitiesAsync(string query, int max = 10);
    
    // מחוזות ונפות
    IReadOnlyList<string> GetDistricts();
    IReadOnlyList<string> GetSubDistricts(string? district = null);
    IReadOnlyList<string> GetLocalitiesBySubDistrict(string subDistrict);
    IReadOnlyList<string> GetLocalitiesByDistrict(string district);
    
    // בדיקת שייכות
    bool IsLocalityInDistrict(string locality, string district);
    bool IsLocalityInSubDistrict(string locality, string subDistrict);
}
```

**רישום:**
```csharp
builder.Services.AddSingleton<ILocalityService, LocalityService>();
```

### 2. קובץ Fallback

```
wwwroot/data/localities.json
```

מבנה:
```json
[
  { "name": "תל אביב - יפו", "district": "תל אביב", "subDistrict": "תל אביב" },
  { "name": "חיפה", "district": "חיפה", "subDistrict": "חיפה" },
  ...
]
```

---

## 🖥️ UI — בחירת עיר ללקוח (`ClientDialog.razor`)

### התנהגות
- **חובה לבחור מהרשימה** — `CoerceValue="true"`
- חיפוש חלקי תוך כדי הקלדה
- אם הערך לא ברשימה — לא מתקבל

### קוד UI

```razor
<MudAutocomplete T="string" Label="עיר" @bind-Value="Client.City"
                 SearchFunc="SearchLocalities"
                 CoerceValue="true"
                 CoerceText="true"
                 ResetValueOnEmptyText="true"
                 Variant="Variant.Outlined"
                 Placeholder="הקלד שם יישוב..." />
```

---

## 🖥️ UI — אזור עבודה למאמן (`CoachDialog.razor` / `Profile.razor`)

### קונספט
המאמן מגדיר אזור עבודה **ברמת מחוז או נפה** (לא ברמת עיר בודדת).  
ניתן לבחור מספר מחוזות/נפות.

### התנהגות
- בחירה מרובה של מחוזות ו/או נפות
- תצוגה כ-Chips עם אפשרות הסרה
- השמירה ב-`availability_area` (JSONB) כאובייקט מובנה

### מבנה JSONB ב-`availability_area`

```json
{
  "districts": ["תל אביב", "מרכז"],
  "subDistricts": ["נפת פתח תקוה", "נפת רמלה"]
}
```

### קוד UI (סקיצה)

```razor
<MudText Typo="Typo.subtitle2" Class="mb-2">אזור עבודה</MudText>

<MudSelect T="string" Label="הוסף מחוז" Variant="Variant.Outlined"
           ValueChanged="AddDistrict" Clearable="true">
    @foreach (var d in availableDistricts)
    {
        <MudSelectItem Value="@d">@d</MudSelectItem>
    }
</MudSelect>

<MudSelect T="string" Label="הוסף נפה" Variant="Variant.Outlined" Class="mt-2"
           ValueChanged="AddSubDistrict" Clearable="true">
    @foreach (var sd in availableSubDistricts)
    {
        <MudSelectItem Value="@sd">@sd</MudSelectItem>
    }
</MudSelect>

<MudStack Row="true" Class="mt-2 flex-wrap">
    @foreach (var d in selectedDistricts)
    {
        <MudChip T="string" Color="Color.Primary" OnClose="() => RemoveDistrict(d)" 
                 Closeable="true">מחוז: @d</MudChip>
    }
    @foreach (var sd in selectedSubDistricts)
    {
        <MudChip T="string" Color="Color.Info" OnClose="() => RemoveSubDistrict(sd)" 
                 Closeable="true">נפה: @sd</MudChip>
    }
</MudStack>
```

---

## 🤝 המלצת מאמן — לוגיקה

### כלל ההמלצה

> אם **עיר הלקוח** שייכת ל**מחוז או נפה** שהמאמן הגדיר כאזור עבודה → המאמן מומלץ לשיבוץ.

### מתודה

```csharp
public bool IsCoachAvailableForClient(Coach coach, ClientOrg client)
{
    if (string.IsNullOrEmpty(client.City)) return false;
    var area = DeserializeArea(coach.AvailabilityArea);
    if (area == null) return false;

    // בדוק אם העיר של הלקוח שייכת למחוז שהמאמן בחר
    foreach (var district in area.Districts)
    {
        if (_localityService.IsLocalityInDistrict(client.City, district))
            return true;
    }

    // בדוק אם העיר של הלקוח שייכת לנפה שהמאמן בחר
    foreach (var subDistrict in area.SubDistricts)
    {
        if (_localityService.IsLocalityInSubDistrict(client.City, subDistrict))
            return true;
    }

    return false;
}
```

### שילוב ב-UI — דיאלוג שיבוץ (`AssignmentDialog.razor`)

כשנבחר לקוח, יוצגו מאמנים מומלצים (שהאזור שלהם מכיל את עיר הלקוח):

```razor
@if (recommendedCoaches.Any())
{
    <MudAlert Severity="Severity.Info" Dense="true" Class="mb-2">
        <MudText Typo="Typo.caption" Style="font-weight: bold;">מאמנים מומלצים (אזור עבודה תואם):</MudText>
        @foreach (var c in recommendedCoaches)
        {
            <MudChip T="string" Size="Size.Small" Color="Color.Success">@c.FirstName @c.LastName</MudChip>
        }
    </MudAlert>
}
```

---

## 📁 מבנה קבצים חדשים

```
Services/
  ILocalityService.cs          ← ממשק
  LocalityService.cs           ← מימוש + cache + fallback
Models/
  Locality.cs                  ← record: Name, District, SubDistrict
  CoachAvailabilityArea.cs     ← record: Districts[], SubDistricts[]
wwwroot/data/
  localities.json              ← fallback סטטי
```

---

## 📊 הערכת effort

| פריט | שעות |
|---|---|
| `LocalityService` — טעינה, cache, fallback, חיפוש | 3h |
| יצירת `localities.json` fallback (סקריפט חד-פעמי) | 1h |
| `ClientDialog` — `MudAutocomplete` עם כפייה | 1h |
| `CoachDialog` + `Profile` — אזור עבודה (מחוז/נפה) | 3h |
| לוגיקת המלצה + UI ב-`AssignmentDialog` | 2h |
| בדיקות edge cases + RTL | 1.5h |
| **סה"כ** | **~11–12h** |

---

## ❗ החלטות שנותרו

| # | שאלה | אפשרויות |
|---|---|---|
| 1 | האם להציג רק מאמנים מומלצים או את כולם עם סימון? | א) כולם + badge "מומלץ" ב) רק מומלצים + "הצג הכל" |
| 2 | האם לאפשר למאמן לבחור גם יישובים בודדים (בנוסף למחוז/נפה)? | מומלץ: לא, כדי לשמור על פשטות |
| 3 | מה קורה כשה-API לא זמין בטעינה ראשונה וגם הfallback ריק? | הצג שדה טקסט חופשי כ-fallback אחרון |
| 4 | האם לשמור cache ב-localStorage או רק ב-memory? | localStorage מומלץ (לא תלוי ברענון) |

---

## 🔄 תרשים זרימה

```
┌──────────────┐     ┌─────────────────┐     ┌──────────────────┐
│  App Start   │────▶│ LocalityService │────▶│  data.gov.il API │
│              │     │  InitializeAsync │     │  (CKAN store)    │
└──────────────┘     └────────┬────────┘     └──────────────────┘
                              │                        │
                              │  (if API fails)        │
                              ▼                        ▼
                     ┌─────────────────┐     ┌──────────────────┐
                     │  localities.json │     │  Returns ~1,300  │
                     │  (fallback)      │     │  localities with │
                     └─────────────────┘     │  district + נפה  │
                                             └──────────────────┘
                              │
              ┌───────────────┼───────────────┐
              ▼               ▼               ▼
     ┌────────────┐  ┌──────────────┐  ┌─────────────────┐
     │ClientDialog│  │ CoachDialog  │  │AssignmentDialog │
     │ City picker│  │ Area picker  │  │ Coach recommend │
     │ (forced)   │  │ (dist/נפה)  │  │ (auto-match)    │
     └────────────┘  └──────────────┘  └─────────────────┘
```
