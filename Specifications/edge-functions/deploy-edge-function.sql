-- ==============================================
-- הגדרת Edge Function Secrets + הוראות פריסה
-- הרצה ב-Supabase SQL Editor
-- ==============================================

-- ============================================================
-- שלב 1: אחסון Secrets ב-Vault (הרץ ב-SQL Editor)
-- ============================================================
-- Supabase Vault מאפשר לשמור סודות בצורה מאובטחת.
-- הערכים כאן נגישים רק דרך SQL — Edge Function משתמש ב-secrets set.

SELECT vault.create_secret(
    'YOUR-BREVO-API-KEY-HERE',
    'BREVO_API_KEY',
    'Brevo API key for sending emails'
);

SELECT vault.create_secret(
    'donot_replay@mop.co.il',
    'BREVO_FROM_EMAIL',
    'Sender email address for Brevo'
);

SELECT vault.create_secret(
    'ORg',
    'BREVO_FROM_NAME',
    'Sender display name for Brevo'
);

-- ============================================================
-- שלב 2: אימות שהסודות נשמרו
-- ============================================================

SELECT name, description, created_at
FROM vault.secrets
WHERE name IN ('BREVO_API_KEY', 'BREVO_FROM_EMAIL', 'BREVO_FROM_NAME');

-- ============================================================
-- שלב 3: פריסת Edge Function (ב-CLI בלבד — לא ב-SQL)
-- ============================================================
-- הפקודות הבאות מורצות בטרמינל, לא ב-SQL Editor:
--
-- 3a. התחברות ל-Supabase CLI:
--     supabase login
--
-- 3b. קישור לפרויקט:
--     Dev:  supabase link --project-ref spyalzbjcfdrkbyqkopa
--     Prod: supabase link --project-ref wwzrsibzpjlckjocjfpn
--
-- 3c. הגדרת Secrets ב-Edge Function runtime:
--     supabase secrets set BREVO_API_KEY="xkeysib-YOUR-BREVO-API-KEY-HERE"
--     supabase secrets set BREVO_FROM_EMAIL="donot_replay_arcan@mop.co.il"
--     supabase secrets set BREVO_FROM_NAME="Arcan Israel"
--
-- 3d. פריסת הפונקציה:
--     supabase functions deploy send-welcome-email --no-verify-jwt
--
-- ============================================================
-- שלב 4: בדיקה (ב-CLI)
-- ============================================================
-- curl -X POST 'https://spyalzbjcfdrkbyqkopa.supabase.co/functions/v1/send-welcome-email' \
--   -H 'Authorization: Bearer sb_publishable_TTHrQfMHDtcJfKWNu9SG-w_eaSeIiyR' \
--   -H 'Content-Type: application/json' \
--   -d '{"toEmail": "test@example.com", "coachFirstName": "בדיקה"}'
