-- ==============================================
-- סקריפט יצירת משתמש Admin
-- הרצה ב-Supabase SQL Editor
-- ==============================================

-- שלב 1: יצירת auth user
-- (יש להריץ דרך Supabase Dashboard → Authentication → Add User)
-- Email: admin@coachmanager.co.il
-- Password: admin
-- לאחר היצירה, העתק את ה-UUID שנוצר ושנה את המשתנה למטה:

DO $$
DECLARE
    v_user_id UUID;
BEGIN
    -- קבל את ה-UUID של המשתמש שנוצר ב-auth.users
    SELECT id INTO v_user_id
    FROM auth.users
    WHERE email = 'admin@coachmanager.co.il';

    IF v_user_id IS NULL THEN
        RAISE EXCEPTION 'לא נמצא משתמש עם admin@coachmanager.co.il ב-auth.users. צור אותו קודם דרך Authentication → Add User';
    END IF;

    -- שלב 2: הכנסה לטבלת users
    INSERT INTO users (user_id, email, phone, is_active)
    VALUES (v_user_id, 'admin@coachmanager.co.il', NULL, true)
    ON CONFLICT (user_id) DO NOTHING;

    -- שלב 3: הכנסה לטבלת coaches
    INSERT INTO coaches (coach_id, first_name, last_name, phone, tax_status)
    VALUES (v_user_id, 'מנהל', 'מערכת', NULL, 'Employee')
    ON CONFLICT (coach_id) DO NOTHING;

    -- שלב 4: שיוך תפקיד Admin (role_id = 1)
    INSERT INTO user_roles (user_id, role_id)
    VALUES (v_user_id, 1)
    ON CONFLICT DO NOTHING;

    RAISE NOTICE 'משתמש Admin נוצר בהצלחה! user_id: %', v_user_id;
END $$;
