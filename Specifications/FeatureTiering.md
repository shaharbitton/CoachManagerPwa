# 🔐 מערכת דרגות פיצ'רים (Feature Tiering) — Coach Manager

**תאריך:** יולי 2026  
**סטטוס:** תכנון — טרם מומש

---

## מטרה

לאפשר שליטה ברמת הפיצ'רים הזמינים לכל לקוח (tenant) במערכת, על בסיס תשלום.  
כל רכיב — כפתור, דוח, עמוד, או פונקציה — ניתן לסימון תחת דרגה מסוימת, וייחסם אוטומטית למי שלא שילם עליו.

---

## דרגות מוצעות (Tiers)

| דרגה | שם | תיאור | מחיר מוצע |
|------|----|--------|-----------|
| 0 | **Basic** | ניהול מאמנים, לקוחות, שיבוצים, דיווחי שעות, חוזים, לוח שנה, הערכות, קבוצות, מרכז הדרכה | ₪X/חודש |
| 1 | **Pro** | דוחות כספיים, ייצוא Excel, ניתוח רווחיות, סקריפט גיבוי | ₪Y/חודש |
| 2 | **Enterprise** | כל הפיצ'רים + API + התאמות אישיות | ₪Z/חודש |

> הדרגות היררכיות: דרגה 1 כוללת את כל מה שבדרגה 0.

---

## מיפוי פיצ'רים לדרגות (דוגמה)

| פיצ'ר | דרגה מינימלית |
|-------|--------------|
| ניהול מאמנים | Basic |
| ניהול לקוחות | Basic |
| ניהול שיבוצים | Basic |
| דיווחי שעות | Basic |
| חוזים וחתימה דיגיטלית | Basic |
| לוח שנה (מאמן + אדמין) | Basic |
| הערכות | Basic |
| מרכז הדרכה | Basic |
| ניהול קבוצות | Basic |
| דוח #1 — תשלום חודשי למאמן | Pro |
| דוח #2 — סיכום שנתי | Pro |
| דוח #3 — רווחיות | Pro |
| דוח #4 — ניצול מכסת שעות | Pro |
| דוח #5 — נוכחות מאמנים | Pro |
| דוח #6 — חיוב ללקוח | Pro |
| דוח #7 — השוואה תקופתית | Pro |
| ייצוא Excel | Pro |
| סקריפט גיבוי | Pro |
| API חיצוני | Enterprise |
| התאמות ממשק (white-label) | Enterprise |

---

## ארכיטקטורה מוצעת

### 1. מודל נתונים

```
טבלה: tenant_subscriptions
├── subscription_id (PK, UUID)
├── tenant_id (FK → tenants / admin user id)
├── tier (int: 0=Basic, 1=Pro, 2=Enterprise)
├── started_at (timestamp)
├── expires_at (timestamp, nullable — null = לצמיתות)
├── is_active (boolean)
├── payment_ref (string, nullable — מזהה תשלום חיצוני)
└── created_at (timestamp)
```

> **הערה:** כרגע המערכת היא single-tenant (אדמין אחד).  
> ה-`tenant_id` יכול להיות ה-`user_id` של האדמין, או שדה קבוע אם יש רק tenant אחד.

### 2. שירות צד-לקוח — `FeatureService`

```csharp
// Services/FeatureService.cs

public enum FeatureTier
{
    Basic = 0,
    Pro = 1,
    Enterprise = 2
}

public interface IFeatureService
{
    /// האם הפיצ'ר זמין לפי הדרגה הנוכחית
    bool IsAvailable(FeatureTier requiredTier);

    /// הדרגה הנוכחית של הלקוח
    FeatureTier CurrentTier { get; }

    /// טעינת הדרגה מה-DB בעת כניסה
    Task LoadAsync();
}
```

**מימוש:**
- בעת login — טוען את ה-tier מ-`tenant_subscriptions`
- שומר ב-memory (singleton/scoped)
- כל בדיקה היא פשוט `currentTier >= requiredTier`

### 3. קומפוננטת עטיפה — `<FeatureGate>`

```razor
@* Components/FeatureGate.razor *@

@if (FeatureService.IsAvailable(RequiredTier))
{
    @ChildContent
}
else if (FallbackContent != null)
{
    @FallbackContent
}
else
{
    <MudTooltip Text="@($"פיצ'ר זה זמין החל מדרגת {GetTierName(RequiredTier)}")">
        <MudChip T="string" Size="Size.Small" Color="Color.Default"
                 Icon="@Icons.Material.Filled.Lock">
            @GetTierName(RequiredTier)
        </MudChip>
    </MudTooltip>
}

@code {
    [Parameter] public FeatureTier RequiredTier { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public RenderFragment? FallbackContent { get; set; }
    [Inject] IFeatureService FeatureService { get; set; } = default!;
}
```

### 4. שימוש בדפים

#### חסימת כפתור בודד:
```razor
<FeatureGate RequiredTier="FeatureTier.Pro">
    <MudButton OnClick="ExportExcel">ייצוא Excel</MudButton>
</FeatureGate>
```

#### חסימת טאב/דוח שלם:
```razor
<FeatureGate RequiredTier="FeatureTier.Pro">
    <MudTabPanel Text="דוח רווחיות">
        ...
    </MudTabPanel>
</FeatureGate>
```

#### חסימת עמוד שלם (בראש ה-page):
```razor
@if (!FeatureService.IsAvailable(FeatureTier.Pro))
{
    <MudAlert Severity="Severity.Info">
        דף זה זמין בדרגת Pro ומעלה.
        <MudButton Variant="Variant.Text" Color="Color.Primary">שדרג עכשיו</MudButton>
    </MudAlert>
    return;
}
```

#### הסתרת פריט ניווט בתפריט:
```razor
@* MainLayout.razor *@
<FeatureGate RequiredTier="FeatureTier.Pro">
    <MudNavLink Href="/admin/reports" Icon="@Icons.Material.Filled.Assessment">דוחות</MudNavLink>
</FeatureGate>
```

---

## דרגת Fallback — מה רואים כשחסום?

שלוש אפשרויות (ניתן לבחור לפי הקשר):

| אפשרות | התנהגות | מתאים ל- |
|---------|---------|----------|
| **הסתרה** | הרכיב לא מוצג כלל | פריטי ניווט, טאבים |
| **נעילה עם תגית** | מוצג אייקון 🔒 + שם הדרגה | כפתורים, פעולות |
| **תצוגה מקדימה** | מוצג blur/preview + כפתור "שדרג" | דוחות, גרפים |

---

## זרימת רכישה / שדרוג

```
1. משתמש לוחץ "שדרג" או רואה רכיב נעול
2. נפתח דיאלוג עם טבלת השוואת דרגות ומחירים
3. בחירת דרגה → הפניה לעמוד תשלום (חיצוני / Stripe / PayPal)
4. לאחר תשלום מוצלח → webhook / callback מעדכן tenant_subscriptions
5. FeatureService.LoadAsync() → דרגה מתעדכנת → UI נפתח
```

> **שלב ראשון:** ניתן לנהל דרגות ידנית מה-DB ללא מערכת תשלום.  
> **שלב שני:** חיבור ל-Stripe/PayPal כשיש צורך.

---

## אבטחה — שכבת RLS (Supabase)

הפיצ'ר הוא בעיקר UI-side (הסתרת/חסימת רכיבים), אבל ניתן להוסיף שכבת הגנה ב-DB:

```sql
-- מניעת גישה לנתוני דוחות למי שאין לו tier מתאים
CREATE POLICY "pro_reports_access" ON time_entries
    FOR SELECT
    USING (
        EXISTS (
            SELECT 1 FROM tenant_subscriptions
            WHERE tenant_id = auth.uid()
            AND tier >= 1
            AND is_active = true
        )
    );
```

> **הערה:** זה אופציונלי — ב-single-tenant עם אדמין אחד, חסימת UI מספיקה.

---

## קבצים חדשים (צפויים במימוש)

| קובץ | תיאור |
|-------|--------|
| `Models/TenantSubscription.cs` | מודל Postgrest |
| `Services/IFeatureService.cs` | ממשק |
| `Services/FeatureService.cs` | מימוש — טעינה מ-DB + בדיקות |
| `Components/FeatureGate.razor` | קומפוננטת עטיפה |
| `Pages/Admin/Upgrade.razor` | דף השוואת דרגות + שדרוג (אופציונלי) |

**רישום ב-`Program.cs`:**
```csharp
builder.Services.AddScoped<IFeatureService, FeatureService>();
```

---

## סיכום מאמץ

| שלב | תיאור | מאמץ |
|-----|--------|------|
| 1 | מודל + טבלה + FeatureService בסיסי | ~2–3 שעות |
| 2 | קומפוננטת FeatureGate | ~1 שעה |
| 3 | סימון פיצ'רים קיימים בדרגות | ~2–3 שעות |
| 4 | דף השוואת דרגות (UI) | ~2 שעות |
| 5 | חיבור תשלום (Stripe/PayPal) | ~4–6 שעות |
| 6 | RLS policies (אופציונלי) | ~1–2 שעות |
| **סה"כ** | | **~12–17 שעות** |

---

## תרחיש עתידי — Multi-Tenant

אם בעתיד המערכת תתמוך בכמה ארגונים:
- כל ארגון = tenant עם `tenant_id`
- כל משתמש משויך ל-tenant
- ה-tier נקבע ברמת ה-tenant (לא ברמת המשתמש)
- RLS policies מתבססים על `tenant_id` + `tier`
