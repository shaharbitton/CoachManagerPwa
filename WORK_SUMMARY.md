# סיכום עבודה - CoachManagerPwa

## 📊 תיאור הפרויקט
**שם**: Coach Manager PWA (Personal Web Application)  
**גרסה נוכחית**: 3.0 (כולל Feature Tiering + Brevo Email API)  
**סטטוס**: MVP פעיל ופועל + עבודה יומיומית  
**טכנולוגיות**: Blazor WebAssembly (.NET 10), Supabase (PostgreSQL), GitHub Pages, MudBlazor

---

## ✅ מה שמוגמר ועובד

### 1️⃣ בסיס הנתונים (Schema)
- ✅ **18 טבלאות** ממומשות עם C# Models
- ✅ **RBAC (Role-Based Access Control)** - Admin / Operations_Lead / Coach
- ✅ **Row Level Security (RLS)** - 9 פוליסיות אבטחה
- ✅ **3 Storage Buckets** עם RLS מתאים
- ✅ **FK Constraints** - הגנה על תהליכי עבודה
- ✅ **Audit Trail** - TrackingCoachRates עם EffectiveFrom/EffectiveTo

### 2️⃣ מסך ניהול (Admin UI)
- ✅ **Dashboard** עם סטטיסטיקות
- ✅ **Coaches** - CRUD מלא + פרטיים בנק + תעודות
- ✅ **Clients** - CRUD מלא + הגדרות
- ✅ **Assignments** - שיבוצים עם לוח שבועי + סינון
- ✅ **TimeReports** - דיווחי שעות עם סטטוסים
  - Draft → Submitted → Admin_Approved → Billed_Paid
  - **חדש**: Rejected סטטוס עם אפשרות לדחיה
- ✅ **Contracts** - הפקת חוזים HTML דינמי + חתימה דיגיטלית
  - **חדש**: אפשרות לצור חוזה מחדש עם נתונים מעודכנים
  - **חדש**: הגנה מפני מחיקת שיבוץ עם חוזה חתום
- ✅ **Documents** - ניהול מסמכים + אחסון בעלון
- ✅ **Rates** - ניהול תעריפים למאמנים
- ✅ **Evaluations** - הערכת מאמנים
- ✅ **Groups** - קבוצות אימון וקטלוג
- ✅ **TrainingHub** - משאבי הדרכה
- ✅ **Notifications** - מערכת התראות

### 3️⃣ מסך מאמן (Coach UI)
- ✅ **Assignments** - שיבוצים עם מכסה שעות
- ✅ **TimeReport** - דיווח שעות עם:
  - ✅ חתימה דיגיטלית (Canvas + PNG)
  - ✅ GPS Verification
  - ✅ תיאור עבודה
- ✅ **CoachDocuments** - מסמכים נדרשים (תעודות, אישורים)
- ✅ **Calendar** - לוח זמנים
- ✅ **Groups** - קבוצות ותלמידים
- ✅ **Profile** - פרופיל אישי + פרטיים בנק
- ✅ **Onboarding** - תהליך ההשלמה 3 שלבים
- ✅ **TrainingHub** - משאבי הדרכה

### 4️⃣ חתימה דיגיטלית
- ✅ **מאמן חותם בשיבוץ** - Canvas אינטראקטיבי + PNG
- ✅ **שמירה ב-Supabase Storage** - עם מסמכי מאמן
- ✅ **צפיה בחוזה** - HTML Preview לפני חתימה
- ✅ **Audit Trail** - תעריך שעה של האירוע

### 5️⃣ דיווחי שעות
- ✅ **דיווח שעות מאמן** - עם תאריך, שעות, תיאור
- ✅ **אישור/דחיית אדמיניסטרטור** - Admin_Approved או Rejected
- ✅ **סטטוסים מלאים**:
  - Draft / Submitted / Admin_Approved / Rejected / Billed_Paid
- ✅ **GPS Verification** - מיקום משמורת (Geolocation API)

### 6️⃣ דוחות ודברים מתקדמים
- ✅ **ReportSystem** - דוחות מגוונים (שימוש שעות, חישוביות, תשלומים)
- ✅ **Feature Tiering** - 3 דרגות (Basic/Pro/Enterprise)
- ✅ **FeatureGate Component** - בקרת גישה לתכונות
- ✅ **Brevo Email API** - שליחת מיילים אוטומטיים

### 7️⃣ הנתונים שלנו עכשיו
- ✅ **RTL (Right-to-Left)** - ערבית/עברית מלאה, MudBlazor RTL
- ✅ **PWA** - Service Worker, manifest, GitHub Pages
- ✅ **Offline** - כמה עבודה, אבל לא סנכרון מלא

### 8️⃣ עדכונים ברמה אחרונה (היום!)
- ✅ **Rejected סטטוס** - דיווחים דחויים יכולים להישמר בסטטוס חדש
- ✅ **Ditch חוזה מחדש** - אם נתונים השתנו, אפשר ליצור חוזה מעודכן
- ✅ **הגנה מהמחיקה** - לא ניתן למחוק שיבוץ עם חוזה חתום
- ✅ **ניהול חוזים** - מחיקת חוזים לא חתומים בעת מחיקת שיבוץ

---

## ❌ פעלים עיקריים שנשארו

### 🔴 חומרה גבוהה (קריטיים לתפעול שטח)
| מס' | פעם | עדיפות | סטטוס |
|---|---|---|---|
| 1 | **Offline/PWA Sync** | גבוהה | ❌ אין מנגנון - LocalStorage + Sync |
| 2 | **Export דוחות PDF/Excel** | גבוהה | ❌ אין יכולת ייצוא |
| 3 | **Prevent חוזה/דיווח כשמסמך חובה פג** | בינוני | ❌ אין ולידציה |
| 4 | **Reject דיווח בשיבוץ** | בינוני | ✅ **עשוי היום!** |
| 5 | **E-Sign חוזה מאמן בשיבוץ** | בינוני | ✅ **במלא** (Remote Signature) |

### 🟡 חומרה בינונית (פונקציונליות + שיפורים)
| מס' | פעם | סטטוס |
|---|---|---|
| 6 | **הפרדת Operations_Lead** - רכז תפעול | ❌ RBAC קיימת אבל UI חסרה |
| 7 | **Automatic Email Notifications** | ❌ Brevo יש, אבל Sync חסר |
| 8 | **SMS/WhatsApp Messaging** | ❌ לא מיומן |
| 9 | **Re-open דיווח לאחר Rejection** | ❌ סטטוס Rejected קיים אבל UI חסרה |
| 10 | **Immutable Billed_Paid Records** | ❌ אין ולידציה לעריכה |

### 🟢 Nice to Have / שלב שני
- ⏳ **Smart Matching Coaches** - סינון אוטומטי לפי תגיות + זמינות
- ⏳ **Favorites בחוזה יד** - מאמנים מועדפים
- ⏳ **Import תלמידים מ-Excel/CSV**
- ⏳ **היסטוריית עבודה בתיקייה לקוח**
- ⏳ **משוב סיכום שיבוץ (Post-Mortem)**
- ⏳ **אימוות אלגוריתמי תר"ז ישראלי**

---

## 🔧 שנויים קודים אחרונים

### היום (תאריך):
1. ✅ **Rejected סטטוס** - הוספה ל-time_entry_status_enum
2. ✅ **Delete Contract + Reopen** - אפשרות לצור חוזה מעודכן
3. ✅ **Protect Signed Contracts** - הגנה מפני מחיקה
4. ✅ **Migration SQL** - שינוי ENUM בבסיס

### ימים האחרונים:
- Feature Tiering (v3.0)
- Brevo Email Integration
- FK Constraints בעבודה
- Coach Rate Linking

---

## 📦 הנושא בעלויות: Git ו-Deployment

### מצב נוכחי:
- 🟢 **GitHub Public** - https://github.com/shaharbitton/CoachManagerPwa
- 🟢 **GitHub Pages** - פורסם כ-PWA ציבורית
- 🟡 **בעיה**: קוד ציבורי + מסמכים שמכילים סודות

### מה צריך לעשות:

#### ח1: ניהול סודות טוב יותר
```
1. לא לאחסן API keys/secrets ב-GitHub
2. להשתמש GitHub Secrets למשתנים סביבתיים
3. Supabase credentials -> appsettings.json (מקומי בלבד) + GitHub Secrets
```

#### ח2: פרסום לשרת פרטי
```
1. בחר שרת ווקל (Hetzner, DigitalOcean, Azure)
2. Configure HTTPS + דומיין משלך
3. Build Release בחלל פרטי
4. Deploy עם CI/CD (GitHub Actions)
```

#### ח3: שינוי דומיין/שיוך
```
1. רכש דומיין משלך (coachmanager.example.com)
2. Configure DNS Redirect
3. Supabase configuration עדכון URL
4. PWA manifest.json - עדכון תחזוקה
```

---

## 📋 רשימת בדיקה לפרסום

### לפני פרסום:
- [ ] העבר קוד ל-Repository פרטי
- [ ] Audit Code למפתחות ולסודות
- [ ] Setup GitHub Secrets
- [ ] Configure Supabase שיוך בסביבה חדשה
- [ ] Build Release Version
- [ ] Test בשרת ביניים

### פרסום:
- [ ] Setup Hosting (בחר שרת)
- [ ] Configure SSL Certificate
- [ ] Deploy Docker/Node Container
- [ ] Configure DNS
- [ ] Test אפליקציה מלאה

### אחרי פרסום:
- [ ] Backup Database
- [ ] RLS Policies דיוק בחדש
- [ ] Monitor Logs
- [ ] Setup Monitoring + Alerts

---

## 🎯 הדרך קדימה (Next Steps)

### שלב 1️⃣: בעדיפות גבוהה (שניות)
1. **Export PDF/Excel** - דוחות שימושיים
2. **Offline Sync** - עבודה בשטח ללא אינטרנט
3. **Repository ניהול** - בטיחות קודים

### שלב 2️⃣: בעדיפות בינוני (שבועות)
4. **Operations_Lead UI** - ממשק מלא
5. **Auto Email Sending** - תזכורות ומיידים
6. **Re-open Reports** - חזרה על דיווחים דחויים

### שלב 3️⃣: בעדיפות נמוכה (חודשים)
7. **Smart Matching**
8. **Advanced Reporting**
9. **Mobile App** (Native/Capacitor)

---

## 📞 יצירת קשר ותמיכה
**Repo**: https://github.com/shaharbitton/CoachManagerPwa  
**Main Branch**: main  
**סטטוס**: פעיל ופעיל

---

**סך הכל**: פרויקט אמין עם ממשק קרוב להשלמה בשרת ציבורי, צריך עדיין העברה לפרטי ודיפלוי לשרת משלך עם דומיין עדכני.

