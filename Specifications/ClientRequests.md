# Client Change Requests — Estimated Effort & Classification

**Date:** July 2026  
**Source:** Client feedback session  
**Last Updated:** July 2026 — status synced with current codebase

---

## 🖥️ UI Changes (Frontend Only)

| #   | Request                                                          | Explanation                                                                                              | Effort           | Status                                                                                                                                              |
| --- | ---------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------- | ---------------- | --------------------------------------------------------------------------------------------------------------------------------------------------- |
| 4   | **Coach: add Work Region**                                       | Display geographic work area for the coach.                                                              | 🟢 Small (1–2h)  | ✅ **בוצע** — `availability_area` (נפות) מוצג כעמודה ב-`Coaches.razor`, עריכה ב-`CoachDialog.razor` וב-`Profile.razor` עם `MudSelect MultiSelection` |
| 1   | **Hamburger menu — close on outside click**                      | Currently the side menu only closes via the X button. Should dismiss when tapping anywhere outside it.   | 🟢 Small (1–2h)  | ✅ **בוצע** — `DrawerVariant.Temporary` עם overlay סגירה + כפתור Pin/Unpin ב-`MainLayout.razor`                                                      |
| 21  | **Group creation: clarify "Assignment" field**                   | UX issue — coaches don't understand what the assignment dropdown means. Needs label/tooltip improvement. | 🟢 Small (1h)    | ✅ **בוצע** — `GroupDialog.razor` מציג שם לקוח + שעות מוקצות במקום UUID, עם label "שיבוץ (לקוח / התקשרות)"                                           |
| 13  | **Time Report: whole hours only (1–6)**                          | Replace free-text/half-hour input with integer-only selector (1–6).                                      | 🟢 Small (1–2h)  | ✅ **בוצע** — `MudNumericField` עם `Step="1m"` ו-`Min="1m" Max="6m"` ב-`TimeReport.razor`                                                            |
| 3   | **Coach: add Available Days + Hours**                            | Show coach's available work days and free hours in coach details.                                        | 🟡 Medium (3–4h) | ✅ **בוצע** — `preferred_schedule` JSON נוסף למודל `Coach`, בוחר ימים+שעות ב-`CoachDialog.razor` וב-`Profile.razor`                                  |
| 11  | **Client: fix document icon click**                              | Document icon on Clients page doesn't open anything — wire up navigation/dialog.                         | 🟢 Small (1–2h)  | ✅ **בוצע** — הוחלף באייקון המלצה למאמנים (`TipsAndUpdates`) שמציג מאמנים מומלצים לפי אזור הלקוח ב-`Clients.razor`                                   |
| 17  | **Rates: fix history not showing new rates**                     | Bug — newly added rates don't appear in rate history view.                                               | 🟡 Medium (2–4h) | ✅ **בוצע** — היסטוריית תעריפים מוצגת לאחר בחירת מאמן ב-`Rates.razor`, רשימה מתרעננת אוטומטית לאחר הוספה/עריכה/מחיקה                                 |
| 26  | **Coach: admin sets salary per framework**                       | Admin can configure salary rate per framework on coach profile.                                          | 🟡 Medium (3–4h) | ✅ **בוצע** — מיושם דרך הקצאת תעריף לשיבוץ ב-`AssignmentDialog.razor` (תעריף ספציפי/חדש לכל מסגרת)                                                   |
| 9   | **Client: add Preferred Days & Hours**                           | Show client's preferred schedule for coaching sessions.                                                  | 🟡 Medium (3–4h) | ✅ **בוצע** — מימוש ימים ושעות מועדפים ב-`ContractDialog.razor` (ברמת הסכם) עם `PreferredSchedule` JSON                                              |
| 5   | **Coach documents: add "Recommendations" type**                  | Add a new document category option in the upload dialog.                                                 | 🟢 Small (1h)    | ✅ **בוצע** — נוסף `Recommendations` ב-`Documents.razor`, `CoachDocuments.razor` ו-`GetDocTypeText`                                                  |
| 7   | **Client: add Accounting Phone**                                 | New phone field on client form.                                                                          | 🟢 Small (1h)    | ✅ **בוצע** — שדה `AccountingPhone` נוסף למודל `ClientOrg` ול-`ClientDialog.razor`                                                                   |
| 23  | **Group students: add note per child**                           | Add a note/comment icon or inline field next to each student in a group.                                 | 🟡 Medium (3–4h) | ✅ **בוצע** — שדה `GeneralNotes` ב-`Student` מוצג בקבוצות, ניתן לעריכה דרך כפתור "הערות" שפותח `StudentDialog` לעדכון                                |
| 20  | **Group creation: add Group Name**                               | Add explicit group name field.                                                                           | 🟢 Small (1h)    | ✅ **בוצע** — שדה `GroupName` קיים במודל `TrainingGroup` ומוצג ב-`Groups.razor`                                                                      |
| 6   | **Client: add Management Phone**                                 | New phone field on client form.                                                                          | 🟢 Small (1h)    | ✅ **בוצע** — שדה `ManagementPhone` נוסף למודל `ClientOrg` ול-`ClientDialog.razor`                                                                   |
| 25  | **Contracts: add Payment Method field**                          | New field — check / bank transfer.                                                                       | 🟢 Small (1–2h)  | ✅ **בוצע** — שדה `PaymentMethod` נוסף למודל `ClientContract`, `MudSelect` ב-`ContractDialog.razor`, עמודה ב-`Contracts.razor`                       |
| 2   | **Coach: add Phone field**                                       | Display phone number on coach profile/card. Field likely exists in model, needs UI binding.              | 🟢 Small (1–2h)  | ✅ **בוצע** — שדה `Phone` נוסף למודל `Coach`, ל-`CoachDialog.razor`, ל-`Profile.razor` ולטבלת מאמנים `Coaches.razor`                                 |
| 8   | **Client: add Secretary Phone**                                  | New phone field on client form.                                                                          | 🟢 Small (1h)    | ✅ **בוצע** — שדה `SecretariatPhone` נוסף למודל `ClientOrg` ול-`ClientDialog.razor`                                                                  |
| 22  | **Group creation: fix error on save**                            | Bug — creating a group currently throws an error. Investigate and fix.                                   | 🟡 Medium (2–4h) | ✅ **בוצע** — תוקן ב-`GroupDialog.razor`: וולידציה ל-`AssignId` ריק (UUID error), הגדרת `ClientId` אוטומטית מהשיבוץ                                  |
| 15  | **Rates: add Framework field**                                   | Add "framework" (מסגרת) to rates form/table.                                                             | 🟡 Medium (2–3h) | ✅ **לא נדרש** — מכוסה ע"י #26: תעריף מקושר לשיבוץ (`AssignId`) שמייצג את המסגרת (לקוח + התקשרות)                                                    |
| 19  | **Group creation: add Class field**                              | Add class/grade to group form.                                                                           | 🟢 Small (1h)    | ✅ **לא נדרש** — שם הכיתה = `GroupName` (שם הקבוצה)                                                                                                   |
| 14  | **Time Reports Approval: show both dates**                       | Display both entry date (תאריך הזנה) and execution date (תאריך ביצוע) in time reports approval view.     | 🟢 Small (1–2h)  | ✅ **בוצע** — `TimeReports.razor` מציג שתי עמודות: תאריך ביצוע (`WorkDate`) ותאריך הזנה (`CreatedAt`)                                                  |
| 16  | **Rates: add Class field**                                       | Add class/grade to rates form/table.                                                                     | 🟢 Small (1–2h)  | ✅ **לא נדרש** — תעריף נשמר לפי שיבוץ (`AssignId`), לא לפי כיתה                                                                                      |
| 10  | **Client: add Allocated Hours display**                          | Show allocated hours per contract (not per client). Click contract row to see proposed schedule.          | 🟢 Small (2h)    | ✅ **בוצע** — `WorkScopeHours` מוצג בעמודה בטבלת חוזים ב-`Contracts.razor`, לחיצה על שורה מציגה לו"ז מועדף (expand row)                              |
| 24  | **Contracts page: show weekly hours**                            | Display weekly hour scope + annual total on the contracts list/detail.                                   | 🟡 Medium (2–3h) | 🟡 חלקי — לו"ז מוצג ב-expand row (טבלה שבועית), שדה `weekly_hours` חסר במודל                                                                        |
| 12  | **Time Report: add Class (כיתה) field**                          | Add class/grade selector to the time entry form.                                                         | 🟢 Small (2h)    | ✅ **בוצע** — `group_id` נוסף למודל `TimeEntry`, בוחר קבוצה (=כיתה) מפולטר לפי שיבוץ ב-`TimeReport.razor`                                            |
| 18  | **Group creation: add Institution Name**                         | Add school/institution name field to group form.                                                         | 🟢 Small (1h)    | ✅ **לא נדרש** — שם המוסד = שם הלקוח, זמין דרך `AssignId` → `ClientId`                                                                               |

**UI Total Estimate (remaining):** ~2–3 שעות (שדה שעות שבועיות #24)

---

## 🏗️ Architecture / Backend Changes

| # | Request | Explanation | Effort | Status |
|---|---------|-------------|--------|--------|
| 1 | **Model: add fields to `coaches` table** | Phone, available_days, available_hours, work_region. Requires DB migration + model update. | 🟡 Medium (2–3h) | ✅ **בוצע** — `phone` נוסף, `availability_area` (`List<string>` נפות), `preferred_schedule` (JSON ימים+שעות) — הכל עם UI מלא |
| 2 | **Model: add fields to `clients` table** | management_phone, accounting_phone, secretary_phone, preferred_days, preferred_hours. DB migration + model. | 🟡 Medium (2–3h) | ✅ **בוצע** — שדות טלפון נוספו למודל `ClientOrg` ול-DB, `preferred_schedule` מיושם ברמת חוזה |
| 3 | **Model: add fields to `time_reports` table** | class_name (כיתה). DB migration + model update. | 🟢 Small (1h) | ✅ **בוצע** — `group_id` נוסף במקום `class_name` (הכיתה = שם הקבוצה, `GroupName`) |
| 4 | **Model: add fields to `rates` table** | framework, class_name. DB migration + model update. | 🟢 Small (1–2h) | ✅ **לא נדרש** — `framework` מכוסה דרך `AssignId`, `class_name` לא נדרש (תעריף לפי שיבוץ, לא לפי כיתה) |
| 5 | **Model: add fields to `groups` table** | institution_name, class_name, group_name. DB migration + model. | 🟢 Small (1–2h) | ✅ **לא נדרש** — `group_name` = כיתה, `institution_name` = שם לקוח (דרך `AssignId` → `ClientId`) |
| 6 | **Model: add fields to `contracts` table** | weekly_hours, annual_hours_total, payment_method. DB migration + model. | 🟢 Small (1–2h) | 🟡 חלקי — `preferred_schedule`, `engagement_name`, `payment_method` קיימים; שדה `weekly_hours` חסר |
| 7 | **Model: add note field to `students` or new `student_notes` table** | Per-student note within a group context. | 🟡 Medium (2–3h) | ✅ **בוצע** — מודל `StudentNote` ו-`Student.GeneralNotes` קיימים |
| 8 | **Excel export of time reports** | Add export service (e.g., ClosedXML or EPPlus) to generate .xlsx from time report data. New service + endpoint/download logic. | 🔴 Large (6–10h) | ⬜ טרם בוצע |
| 9 | **Allocation model: weekly + annual allocation** | Rethink allocated_hours to support both weekly and annual caps. Schema change + business logic. | 🔴 Large (8–12h) | ⬜ טרם בוצע |
| 10 | **Allocation: exceptional allocation without contract** | Allow ad-hoc allocations not tied to a contract. New flag/logic. | 🟡 Medium (3–5h) | ⬜ טרם בוצע |
| 11 | **Allocation: cancelled session rescheduling** | If a session is cancelled, hours return to the pool for reuse on another date. Business logic change. | 🟡 Medium (4–6h) | ⬜ טרם בוצע |
| 12 | **Schedule/Calendar system** | New feature: weekly/monthly calendar for coaches and admin. New table(s) + full UI. Major feature. | 🔴 Large (20–30h) | ⬜ טרם בוצע |
| 13 | **Contract PDF generation from template** | After filling contract details, generate a standard contract document. Requires PDF library + template engine. | 🔴 Large (8–12h) | ✅ **בוצע** — מיושם כ-HTML template עם חתימה דיגיטלית: `ContractGeneratorService`, `SignContract.razor`, חותמת עסקית embedded resource, שמירה אוטומטית למסמכי מאמן |
| 14 | **Document type enum: add "Recommendations"** | Extend document category enum/list in schema. | 🟢 Small (1h) | ✅ **בוצע** — נוסף לכל דיאלוגי העלאה ולפונקציות `GetDocTypeText` |

**Architecture Total Estimate (remaining):** ~38–61 hours (מתוך ~60–90 מקורי)

---

## 📊 Summary

| Category | Items Total | Completed | Partial | Remaining | Est. Remaining Hours |
|----------|-------------|-----------|---------|-----------|----------------------|
| UI-only changes | 26 | 24 | 1 | 1 | ~2–3h |
| Architecture/Backend changes | 14 | 9 | 1 | 4 | ~38–61h |
| **Total** | **40** | **33** | **2** | **5** | **~40–64h** |

### ✅ Completed Items
- UI #1: תפריט המבורגר — סגירה בלחיצה מחוץ + כפתור Pin/Unpin ב-`MainLayout.razor`
- UI #2: טלפון מאמן — שדה `Phone` במודל, בדיאלוג, בפרופיל ובטבלת מאמנים
- UI #3: ימים ושעות מועדפים למאמן — `preferred_schedule` JSON ב-`CoachDialog.razor` וב-`Profile.razor`
- UI #4: אזור עבודה למאמן — `availability_area` (נפות) ב-`CoachDialog.razor`, `Profile.razor` ועמודה ב-`Coaches.razor`
- UI #5: סוג מסמך "המלצות" — נוסף לכל דיאלוגי העלאה ול-`GetDocTypeText`
- UI #6–8: טלפונים לקוח — `ManagementPhone`, `AccountingPhone`, `SecretariatPhone` במודל וב-`ClientDialog`
- UI #9: ימים ושעות מועדפים ללקוח — מיושם ב-`ContractDialog.razor` עם JSON schedule
- UI #11: אייקון המלצה — מציג מאמנים מומלצים לפי אזור (נפה) עם `TipsAndUpdates` ב-`Clients.razor`
- UI #13: שעות שלמות בלבד — `MudNumericField` עם Step=1, Min=1, Max=6
- UI #20: שם קבוצה — שדה `GroupName` קיים במודל ובתצוגה
- UI #21: הבהרת שדה שיבוץ — מציג שם לקוח + שעות במקום UUID
- UI #22: תיקון שגיאה ביצירת קבוצה — וולידציית UUID + הגדרת ClientId אוטומטית
- UI #17: היסטוריית תעריפים — מוצגת לאחר בחירת מאמן, מתרעננת אוטומטית לאחר שינויים
- UI #23: הערות לתלמיד — שדה `GeneralNotes` ב-`StudentDialog`, מוצג בקבוצות, עריכה דרך כפתור "הערות"
- UI #25: אמצעי תשלום בחוזה — `PaymentMethod` במודל, `MudSelect` ב-`ContractDialog`, עמודה ב-`Contracts.razor`
- UI #12: כיתה בדיווח שעות — group_id נוסף למודל, בוחר קבוצה (=כיתה) מפולטר לפי שיבוץ
- UI #14: תאריכי דיווח — שתי עמודות: תאריך ביצוע + תאריך הזנה ב-`TimeReports.razor`
- UI #15: מסגרת בתעריפים — לא נדרש, מכוסה ע"י #26 (תעריף מקושר לשיבוץ = מסגרת)
- UI #16: כיתה בתעריפים — לא נדרש, תעריף נשמר לפי שיבוץ ולא לפי כיתה
- UI #18: שם מוסד בקבוצה — לא נדרש, שם המוסד = שם הלקוח (דרך השיבוץ)
- UI #19: כיתה בקבוצה — לא נדרש, שם הכיתה = שם הקבוצה (GroupName)
- UI #26: תעריף לפי מסגרת — מיושם דרך הקצאת תעריף לשיבוץ ב-`AssignmentDialog.razor`
- Backend #1: שדות מאמן — `phone`, `availability_area` (List<string> נפות), `preferred_schedule` (JSON)
- Backend #2: שדות לקוח — טלפונים נוספו למודל ול-DB
- Backend #7: טבלת `student_notes` — מודל קיים עם `StudentNote` ו-`Student.GeneralNotes`
- Backend #14: סוג מסמך "המלצות" — נוסף לכל הממשקים
- Backend #13: יצירת חוזה מתבנית — `ContractGeneratorService` מייצר HTML מ-template, חתימה דיגיטלית ב-`SignContract.razor`, חותמת עסקית embedded, שמירת חוזה חתום למסמכי מאמן

### 🔧 Additional Improvements (not in original spec)
- **שירות יישובים (LocalityService)**: קובץ סטטי `localities.json` עם 1,272 יישובים מ-data.gov.il, חיפוש autocomplete, מיפוי נפות
- **עיר לקוח — בחירה מחייבת**: שדה עיר הפך ל-`MudAutocomplete` עם `CoerceValue=true` (חובה לבחור מהרשימה)
- **המלצת מאמנים לפי אזור**: לחיצה על אייקון בדף לקוחות מציגה מאמנים שאזור הזמינות שלהם תואם לנפת הלקוח
- **המלצת מאמנים בשיבוץ**: `AssignmentDialog.razor` מציג הערה עם מאמנים מומלצים בעת בחירת לקוח
- **תעריף ברירת מחדל בשיבוץ**: מצב "תעריף ברירת מחדל" שמתעדכן אוטומטית עם שכר המאמן העדכני
- **קבוצות — סינון לפי שיבוץ**: נוסף `MudSelect` פילטר ב-`CoachGroups.razor` עם תמיכה ב-query parameter מדף שיבוצים
- **קבוצות — מצב טעינה**: spinner בזמן טעינת נתונים במקום הצגת UUID
- **קבוצות אדמין — תצוגה מורחבת**: עמודות לקוח + התקשרות ב-`Groups.razor`, סינון לפי שיבוץ (שם התקשרות + שם לקוח)
- **קבוצות מאמן — תווית שיבוץ**: `CoachGroups.razor` מציג שם התקשרות + שם לקוח בפילטר במקום שם לקוח + שעות
- **דיווח שעות — סינון לפי שיבוץ וקבוצה**: TimeReport.razor מציג שם התקשרות+לקוח בפילטר שיבוץ, בוחר קבוצה מפולטרת, query parameter מדף שיבוצים
- **תפריט צד — כפתור הצמדה (Pin)**: מאפשר מעבר בין תפריט קבוע לתפריט זמני
- **לו"ז שיבוץ**: שדה `schedule` (JSON) נוסף למודל `Assignment`, עורך ימים+שעות ב-`AssignmentDialog.razor`, תצוגת expand row בטבלת שיבוצים
- **תצוגת לו"ז שבועית**: טבלה שבועית (RTL, כל 7 ימים, ✓ בלבד) ב-`Coaches.razor`, `Contracts.razor` ו-`Assignments.razor` — לחיצה על שורה מציגה לו"ז
- **מערכת חוזי שיבוץ + חתימה דיגיטלית**: מודל `CoachAssignmentContract`, שירות `ContractGeneratorService` עם HTML template, דף `SignContract.razor` עם signature pad (JavaScript), חותמת עסקית כ-embedded resource
- **תאריך חתימה דינמי**: מציג את התאריך של היום בתצוגת חוזה, נעול על תאריך החתימה בפועל
- **שמירת חוזה חתום למסמכי מאמן**: לאחר חתימה, החוזה נשמר אוטומטית ל-Supabase Storage ונוצר רשומת `CoachDocument` (סוג: "חוזה חתום")
- **התראות מאמן**: דף `CoachNotifications.razor` עם `NotificationService` — שליחת התראות על חוזים חדשים לחתימה
- **Onboarding מאמן**: דף `Onboarding.razor` לתהליך קליטה ראשוני

### 🟡 Partially Completed
- Backend #6: שדות חוזים — `preferred_schedule`, `engagement_name`, `payment_method` קיימים; שדה `weekly_hours` חסר
- UI #24: לו"ז מוצג בטבלה שבועית (expand row), שדה שעות שבועיות חסר במודל

---

## 🎯 Recommended Priority (Updated)

### Phase 1 — Quick wins (UI fixes + small model changes) ✅ הושלם
- ~~Fix bugs: group creation error (#22), rates history (#17), document icon click (#11)~~
- ~~Add simple fields: phones (#6–8), class (#12, #16, #19), framework (#15), payment method (#25)~~
- ~~Hamburger menu dismiss behavior (#1)~~
- ~~Whole-hours-only constraint (#13)~~
- ~~Add "Recommendations" doc type (#5, Backend #14)~~
- ~~Allocated hours display (#10), show report date (#14)~~

### Phase 2 — Medium features
- Excel export (Backend #8)
- ~~Complete per-student notes UI (#23)~~ ✅
- Weekly hours field on contracts (#24) — שדה `weekly_hours` חסר במודל
- ~~Contract PDF generation (Backend #13)~~ ✅ (HTML template + חתימה דיגיטלית)
- ~~Salary per framework — admin (#26)~~ ✅
- ~~Coach fields: phone (#2), days/hours (#3), work region (#4, Backend #1)~~ ✅
- ~~לו"ז שיבוץ — `schedule` JSON ב-`Assignment` + עורך + expand row~~ ✅

### Phase 3 — Major features
- Schedule/Calendar system (Backend #12 — biggest effort)
- Weekly + annual allocation model (Backend #9)
- Cancelled session hour reuse logic (Backend #11)
- Exceptional allocation (Backend #10)

---

## ❓ Open Questions (Require Client Decision)

| # | Question |
|---|----------|
| 1 | Weekly vs. annual allocation — should both always be active, or configurable per client? |
| 2 | Cancelled session — automatic return to pool, or admin approval required? |
| 3 | Schedule — is this a read-only view or should it support drag-and-drop scheduling? |
| 4 | Exceptional allocations — any limit or approval flow? |
| ~~5~~ | ~~"Framework" (מסגרת) — לא רלוונטי, מכוסה דרך שיבוץ (Assignment → Contract)~~ |
