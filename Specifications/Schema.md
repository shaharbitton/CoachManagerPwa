יצירת סכמת נתונים ראשונית - PostgreSQL / Supabase DDL
גרסה: 1.0 (תואם לאפיון V1.6) | תאריך: יולי 2026 | מערכת יעד: Supabase (PostgreSQL 15+)

1. מבוא והנחיות הרצה ב-Supabase
מסמך זה מכיל את סקריפט ה-SQL המלא (Data Definition Language) המיועד להקמה ראשונית של מסד הנתונים עבור מערכת ניהול המאמנים, הלקוחות ומאגר הידע הפדגוגי. הסקריפט נכתב ותוכנן במיוחד עבור סביבת Supabase המבוססת על מנוע PostgreSQL, ומיישם הלכה למעשה את כל העקרונות הארכיטקטוניים שהוגדרו במסמך האפיון V1.6.
דגשים ארכיטקטוניים המיושמים בקוד:
שימוש ב-UUID: כל המפתחות הראשיים (Primary Keys) מבוססים על מזהי UUID ייחודיים בעזרת ההרחבה uuid-ossp, לתמיכה מושלמת בסנכרון Offline ומניעת התנגשויות במובייל (PWA).
הפרדה פיננסית וקשר כפול ב-coach_rates: טבלת תעריפי השכר מקושרת מחד למאמן (coach_id) ומאידך לשיבוץ (assign_id כאופציונלי / Nullable), מה שמאפשר להגדיר שכר בסיס כללי לצד שכר ייעודי והיסטורי לפרויקט ספציפי.
פרטיות ואבטחה (IP Removal): בהתאם להחלטה המעודכנת, טבלת החתימות הדיגיטליות (signed_documents) אינה שומרת את כתובת ה-IP של החותם.
אכיפת חתימת לקוח בשטח: שדה החתימה (signature_url) בטבלת דיווחי השעות מוגדר כשדה חובה (NOT NULL) ברמת מסד הנתונים עבור דיווחים שאושרו.
הנחיות ביצוע: יש להעתיק את קטעי הקוד להלן (לפי הסדר) ולהריץ אותם בחלון ה-SQL Editor בממשק הניהול של Supabase.

2. הגדרת הרחבות (Extensions) וסוגי Enums מותאמים
לפני יצירת הטבלאות, נגדיר את ההרחבה ליצירת UUID ואת רשימות הערכים הסגורות (Enumerated Types) לשמירה על שלמות הנתונים (Data Integrity):
שם ה-Enum
ערכים אפשריים
שימוש במערכת
 
tax_status_enum
Employee, Exempt_Dealer, Licensed_Dealer
הגדרת המעמד המיסויי של המאמן לצורך הפקת תלושי שכר או קבלת חשבוניות.
doc_type_enum
National_ID_Card, Police_Clearance, Certification, Tax_Withholding, Bank_Confirmation
סיווג מסמכי חובה שהמאמן נדרש להעלות לתיק האישי שלו.
doc_status_enum
Pending_Review, Approved, Rejected
סטטוס אישור המסמכים ע"י רכז התפעול או משאבי אנוש.
client_type_enum
School, Community_Center, NGO, Private_Client
קיטרוג וסיווג הלקוחות והמוסדות של החברה.
assignment_status_enum
Active, Completed, Cancelled
סטטוס פעילות של שיבוץ מאמן לפרויקט מול מוסד.
time_entry_status_enum
Pending_Signature, Client_Approved, Admin_Approved, Billed_Paid
מחזור החיים של דיווח שעות: מחתימה בשטח, דרך אישור הנהלה ועד חיוב ושכר.

-- Enable UUID Extension
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Create Custom Enums
CREATE TYPE tax_status_enum AS ENUM ('Employee', 'Exempt_Dealer', 'Licensed_Dealer');
CREATE TYPE doc_type_enum AS ENUM ('National_ID_Card', 'Police_Clearance', 'Certification', 'Tax_Withholding', 'Bank_Confirmation');
CREATE TYPE doc_status_enum AS ENUM ('Pending_Review', 'Approved', 'Rejected');
CREATE TYPE client_type_enum AS ENUM ('School', 'Community_Center', 'NGO', 'Private_Client');
CREATE TYPE assignment_status_enum AS ENUM ('Active', 'Completed', 'Cancelled');
CREATE TYPE time_entry_status_enum AS ENUM ('Pending_Signature', 'Client_Approved', 'Admin_Approved', 'Billed_Paid');



3. שכבת זהויות, משתמשים והרשאות (RBAC)
שכבה זו מנהלת את פרטי המשתמשים במערכת ומקשרת אותם לתפקידים השונים (רבים-לרבים). טבלת users תתוחזק לרוב בסנכרון מול טבלת האותנטיקציה המובנית auth.users של Supabase בעזרת Trigger אוטומטי בעת הרשמה.
-- 1. Users Table (Core App Users)
CREATE TABLE users (
    user_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    email VARCHAR(255) UNIQUE NOT NULL,
    phone VARCHAR(50),
    is_active BOOLEAN DEFAULT true NOT NULL,
    created_at TIMESTAMPTZ DEFAULT NOW() NOT NULL
);

-- 2. Roles Table (System Roles)
CREATE TABLE roles (
    role_id SERIAL PRIMARY KEY,
    role_name VARCHAR(50) UNIQUE NOT NULL,
    description TEXT
);

-- Insert Default Roles
INSERT INTO roles (role_name, description) VALUES
('Admin', 'מנהל מערכת והנהלת חשבונות - גישה מלאה לכלל הישויות והשכר'),
('Operations_Lead', 'רכז/מנהל תפעול - שיבוצים, ניהול לקוחות ומאגר ידע'),
('Coach', 'מאמן בשטח - דיווח שעות, החתמת לקוחות וניהול קבוצות'),
('Client_Rep', 'נציג לקוח/מוסד - אישור שעות וצפייה בשיבוצים');

-- 3. UserRoles Table (Many-to-Many Bridge)
CREATE TABLE user_roles (
    user_id UUID NOT NULL REFERENCES users(user_id) ON DELETE CASCADE,
    role_id INTEGER NOT NULL REFERENCES roles(role_id) ON DELETE RESTRICT,
    assigned_at TIMESTAMPTZ DEFAULT NOW() NOT NULL,
    PRIMARY KEY (user_id, role_id)
);



4. שכבת פרופיל מאמן, משאבי אנוש ושכר (Coaches & HR)
שכבה זו מרכזת את כל המידע האישי, המקצועי והפיננסי על המאמנים. שים לב לבידוד פרטי הבנק בטבלה נפרדת (coach_bank_details) ולקשר הכפול בטבלת התעריפים (coach_rates).
-- 4. Coaches Profile Table
CREATE TABLE coaches (
    coach_id UUID PRIMARY KEY REFERENCES users(user_id) ON DELETE CASCADE,
    first_name VARCHAR(100) NOT NULL,
    last_name VARCHAR(100) NOT NULL,
    national_id VARCHAR(20) UNIQUE NOT NULL,
    birth_date DATE,
    age INTEGER,
    occupation VARCHAR(150),
    tax_status tax_status_enum NOT NULL DEFAULT 'Employee',
    hard_skills JSONB DEFAULT '[]'::jsonb,
    availability_area JSONB DEFAULT '{}'::jsonb,
    created_at TIMESTAMPTZ DEFAULT NOW() NOT NULL
);

-- 5. Coach Bank Details Table (Secured & Isolated RLS Entity)
CREATE TABLE coach_bank_details (
    bank_info_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    coach_id UUID UNIQUE NOT NULL REFERENCES coaches(coach_id) ON DELETE CASCADE,
    bank_name VARCHAR(100) NOT NULL,
    branch_number VARCHAR(20) NOT NULL,
    account_number VARCHAR(50) NOT NULL,
    beneficiary_name VARCHAR(150) NOT NULL,
    updated_at TIMESTAMPTZ DEFAULT NOW() NOT NULL
);

-- 6. Coach Documents Table (Admin / HR Compliance)
CREATE TABLE coach_documents (
    doc_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    coach_id UUID NOT NULL REFERENCES coaches(coach_id) ON DELETE CASCADE,
    doc_type doc_type_enum NOT NULL,
    file_url TEXT NOT NULL,
    expiration_date DATE,
    status doc_status_enum DEFAULT 'Pending_Review' NOT NULL,
    created_at TIMESTAMPTZ DEFAULT NOW() NOT NULL
);

-- 7. Signed Documents Table (E-Sign Archive - Without IP)
CREATE TABLE signed_documents (
    signed_doc_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    coach_id UUID NOT NULL REFERENCES coaches(coach_id) ON DELETE CASCADE,
    template_title VARCHAR(200) NOT NULL,
    signed_pdf_url TEXT NOT NULL,
    signed_at TIMESTAMPTZ DEFAULT NOW() NOT NULL
);

-- 8. Coach Evaluations Table (Secret HR Only)
CREATE TABLE coach_evaluations (
    eval_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    coach_id UUID NOT NULL REFERENCES coaches(coach_id) ON DELETE CASCADE,
    reviewer_id UUID NOT NULL REFERENCES users(user_id) ON DELETE RESTRICT,
    eval_date DATE NOT NULL DEFAULT CURRENT_DATE,
    score INTEGER CHECK (score >= 1 AND score <= 5),
    strengths TEXT,
    weaknesses TEXT,
    notes TEXT
);

-- 9. Coach Attributes Table (Persona Tags - Secret HR)
CREATE TABLE coach_attributes (
    attribute_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    coach_id UUID NOT NULL REFERENCES coaches(coach_id) ON DELETE CASCADE,
    tag_name VARCHAR(100) NOT NULL,
    added_by UUID REFERENCES users(user_id) ON DELETE SET NULL,
    created_at TIMESTAMPTZ DEFAULT NOW() NOT NULL
);



5. שכבת לקוחות, חוזים, שיבוצים ותעריפים (Clients, Contracts & Pay Rates)
בשכבה זו מיושם מודל ההתקשרות הכפול המפריד בין תעריף החיוב שהלקוח משלם לעסק (בטבלת client_contracts החסומה למאמנים) לבין שכר המאמן (בטבלת coach_rates). טבלת coach_rates נוצרת כעת לאחר ש-assignments קיימת, כדי לאפשר את המפתח הזר הכפול.
-- 10. Clients Table (Institutions & Schools)
CREATE TABLE clients (
    client_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    client_name VARCHAR(200) NOT NULL,
    tax_id VARCHAR(50),
    client_type client_type_enum NOT NULL DEFAULT 'School',
    address VARCHAR(255),
    city VARCHAR(100),
    management_email VARCHAR(255),
    accounting_email VARCHAR(255),
    secretariat_email VARCHAR(255),
    is_active BOOLEAN DEFAULT true NOT NULL,
    created_at TIMESTAMPTZ DEFAULT NOW() NOT NULL
);

-- 11. Client Contracts Table (RLS Secret - Billing Rates)
CREATE TABLE client_contracts (
    contract_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    client_id UUID NOT NULL REFERENCES clients(client_id) ON DELETE CASCADE,
    billing_rate_per_hour NUMERIC(10, 2) NOT NULL,
    work_scope_hours NUMERIC(8, 2),
    payment_terms VARCHAR(100),
    start_date DATE NOT NULL,
    end_date DATE,
    created_at TIMESTAMPTZ DEFAULT NOW() NOT NULL
);

-- 12. Assignments Table (Projects / Bridge between Client and Coach)
CREATE TABLE assignments (
    assign_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    client_id UUID NOT NULL REFERENCES clients(client_id) ON DELETE RESTRICT,
    coach_id UUID NOT NULL REFERENCES coaches(coach_id) ON DELETE RESTRICT,
    contract_id UUID REFERENCES client_contracts(contract_id) ON DELETE SET NULL,
    allocated_hours NUMERIC(8, 2),
    status assignment_status_enum DEFAULT 'Active' NOT NULL,
    satisfaction_score INTEGER CHECK (satisfaction_score >= 1 AND satisfaction_score <= 10),
    rehire_recommended BOOLEAN,
    created_at TIMESTAMPTZ DEFAULT NOW() NOT NULL
);

-- 13. Coach Rates Table (Dual Link: Coach & Nullable Assignment)
-- מאפשר להגדיר תעריף שכר בסיסי למאמן (assign_id = NULL) או תעריף שכר ייעודי לשיבוץ ספציפי
CREATE TABLE coach_rates (
    rate_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    coach_id UUID NOT NULL REFERENCES coaches(coach_id) ON DELETE CASCADE,
    assign_id UUID REFERENCES assignments(assign_id) ON DELETE CASCADE,
    hourly_rate NUMERIC(10, 2) NOT NULL,
    effective_from DATE NOT NULL DEFAULT CURRENT_DATE,
    effective_to DATE,
    created_at TIMESTAMPTZ DEFAULT NOW() NOT NULL
);



6. שכבת תפעול שוטף ומאגר ידע (Operations & Training Hub)
טבלת דיווחי השעות (time_entries) היא לב המערכת התפעולית. שמירת תמונת החתימה (signature_url) מוגדרת כשדה חובה כאשר הדיווח בסטטוס מאושר, וקואורדינטות ה-GPS נשמרות כשדות רשות.
-- 14. Time Entries Table (Field Tracking & Mandatory Signatures)
CREATE TABLE time_entries (
    entry_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    assign_id UUID NOT NULL REFERENCES assignments(assign_id) ON DELETE RESTRICT,
    work_date DATE NOT NULL DEFAULT CURRENT_DATE,
    hours_reported NUMERIC(5, 2) NOT NULL CHECK (hours_reported > 0),
    status time_entry_status_enum DEFAULT 'Pending_Signature' NOT NULL,
    signature_url TEXT NOT NULL, -- שדה חובה לאישור והחתמת הלקוח בשטח
    signer_name VARCHAR(150),
    signer_role VARCHAR(100),
    gps_verified BOOLEAN DEFAULT false NOT NULL,
    latitude NUMERIC(10, 7),
    longitude NUMERIC(10, 7),
    description TEXT,
    created_at TIMESTAMPTZ DEFAULT NOW() NOT NULL
);

-- 15. Training Resources Table (Knowledge Hub & Pedagogical Library)
CREATE TABLE training_resources (
    resource_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    title VARCHAR(200) NOT NULL,
    description TEXT,
    skill_category VARCHAR(100) NOT NULL,
    target_age_group VARCHAR(100),
    attachment_url TEXT,
    created_by UUID REFERENCES users(user_id) ON DELETE SET NULL,
    created_at TIMESTAMPTZ DEFAULT NOW() NOT NULL
);



7. שכבת קבוצות משתתפים ומעקב אישי (Training Groups & Students)
שכבה זו מאפשרת למאמן להקים קבוצות ולנהל יומן מעקב אישי עבור כל משתתף. קישור השדה coach_id ישירות לטבלת הקבוצות הוא קריטי ליישום חוקי האבטחה (RLS), כדי שמאמנים לא יוכלו לצפות בקבוצות של קולגות.
-- 16. Groups Table (Training Groups managed by Coach)
CREATE TABLE groups (
    group_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    assign_id UUID NOT NULL REFERENCES assignments(assign_id) ON DELETE CASCADE,
    client_id UUID NOT NULL REFERENCES clients(client_id) ON DELETE CASCADE,
    coach_id UUID NOT NULL REFERENCES coaches(coach_id) ON DELETE RESTRICT,
    group_name VARCHAR(150) NOT NULL,
    created_at TIMESTAMPTZ DEFAULT NOW() NOT NULL
);

-- 17. Students Table (Participants List)
CREATE TABLE students (
    student_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    group_id UUID NOT NULL REFERENCES groups(group_id) ON DELETE CASCADE,
    first_name VARCHAR(100) NOT NULL,
    last_name VARCHAR(100) NOT NULL,
    general_notes TEXT,
    created_at TIMESTAMPTZ DEFAULT NOW() NOT NULL
);

-- 18. Student Notes Table (Personal Evaluation & Follow-up Log)
CREATE TABLE student_notes (
    note_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    student_id UUID NOT NULL REFERENCES students(student_id) ON DELETE CASCADE,
    coach_id UUID NOT NULL REFERENCES coaches(coach_id) ON DELETE RESTRICT,
    lesson_date DATE NOT NULL DEFAULT CURRENT_DATE,
    lesson_topic VARCHAR(200),
    note_text TEXT NOT NULL,
    created_at TIMESTAMPTZ DEFAULT NOW() NOT NULL
);



8. יצירת אינדקסים (Indexes) לשיפור ביצועים ושאילתות
על מנת להבטיח ביצועים מהירים באפליקציית המובייל גם תחת עומס נתונים וכאשר המכשיר מבצע סנכרון לאחר עבודה ב-Offline, מוגדרים אינדקסים על כל המפתחות הזרים והשדות המשמשים באופן תדיר לסינון בשאילתות:
-- Performance Indexes for Foreign Keys & Filtering
CREATE INDEX idx_user_roles_user_id ON user_roles(user_id);
CREATE INDEX idx_user_roles_role_id ON user_roles(role_id);
CREATE INDEX idx_coach_documents_coach_id ON coach_documents(coach_id);
CREATE INDEX idx_coach_documents_status ON coach_documents(status);
CREATE INDEX idx_signed_documents_coach_id ON signed_documents(coach_id);
CREATE INDEX idx_coach_evaluations_coach_id ON coach_evaluations(coach_id);
CREATE INDEX idx_coach_attributes_coach_id ON coach_attributes(coach_id);
CREATE INDEX idx_client_contracts_client_id ON client_contracts(client_id);
CREATE INDEX idx_assignments_coach_id ON assignments(coach_id);
CREATE INDEX idx_assignments_client_id ON assignments(client_id);
CREATE INDEX idx_assignments_status ON assignments(status);
CREATE INDEX idx_coach_rates_coach_id ON coach_rates(coach_id);
CREATE INDEX idx_coach_rates_assign_id ON coach_rates(assign_id);
CREATE INDEX idx_time_entries_assign_id ON time_entries(assign_id);
CREATE INDEX idx_time_entries_work_date ON time_entries(work_date);
CREATE INDEX idx_time_entries_status ON time_entries(status);
CREATE INDEX idx_training_resources_category ON training_resources(skill_category);
CREATE INDEX idx_groups_coach_id ON groups(coach_id);
CREATE INDEX idx_groups_assign_id ON groups(assign_id);
CREATE INDEX idx_students_group_id ON students(group_id);
CREATE INDEX idx_student_notes_student_id ON student_notes(student_id);
CREATE INDEX idx_student_notes_coach_id ON student_notes(coach_id);



9. הנחיות ליישום Row Level Security (RLS) ב-Supabase
מנוע האבטחה של Supabase מחייב הפעלה מפורשת של Row Level Security על כל טבלה. להלן טבלה המסכמת את מדיניות האבטחה (Policies) הנדרשת עבור כל שכבה, ולאחריה קוד SQL לדוגמה להפעלה מהירה:
טבלה / שכבה
גישת מנהלים / תפעול (Admin / Ops)
גישת מאמן בשטח (Coach)
 
coaches, coach_rates
גישה מלאה (ALL / CRUD) לכלל הרשומות והתעריפים במערכת.
צפייה ועריכה של הפרופיל האישי וצפייה בתעריפי השכר המשויכים ל-auth.uid() בלבד.
coach_bank_details
גישה מלאה להנהלת חשבונות ואדמין בלבד לצורך ביצוע העברות שכר.
צפייה ועריכה של חשבון הבנק האישי בלבד. חסום לחלוטין למאמנים אחרים.
client_contracts
גישה מלאה לניהול תעריפי החיוב של הלקוחות.
אין גישה כלל (No Access). מניעת זליגת מידע על רווחיות ותעריפי לקוח.
coach_evaluations, coach_attributes
גישה מלאה לרכזי התפעול והנהלה להזנת ביקורות שטח ותגיות אופי.
אין גישה כלל (No Access). מידע סודי לשידוך וסינון פנימי בלבד (HR Only).
groups, students, student_notes
גישה מלאה לצפייה וניהול כלל הקבוצות והמשתתפים במוסדות.
גישה (CRUD) אך ורק לרשומות המקושרות ל-coach_id התואם ל-auth.uid() של המאמן המחובר.

-- 1. Enable RLS on all sensitive tables
ALTER TABLE coaches ENABLE ROW LEVEL SECURITY;
ALTER TABLE coach_bank_details ENABLE ROW LEVEL SECURITY;
ALTER TABLE coach_rates ENABLE ROW LEVEL SECURITY;
ALTER TABLE client_contracts ENABLE ROW LEVEL SECURITY;
ALTER TABLE coach_evaluations ENABLE ROW LEVEL SECURITY;
ALTER TABLE coach_attributes ENABLE ROW LEVEL SECURITY;
ALTER TABLE groups ENABLE ROW LEVEL SECURITY;
ALTER TABLE students ENABLE ROW LEVEL SECURITY;
ALTER TABLE student_notes ENABLE ROW LEVEL SECURITY;

-- 2. Sample Policy: Coach Bank Details (Only Owner or Admin can view/edit)
CREATE POLICY "Coach can manage own bank details" ON coach_bank_details
    FOR ALL
    USING (auth.uid() = coach_id)
    WITH CHECK (auth.uid() = coach_id);

-- 3. Sample Policy: Client Contracts (Blocked for Coaches, Allowed for Admins/Ops)
CREATE POLICY "Admins and Ops can manage client contracts" ON client_contracts
    FOR ALL
    USING (
        EXISTS (
            SELECT 1 FROM user_roles ur
            JOIN roles r ON ur.role_id = r.role_id
            WHERE ur.user_id = auth.uid() 
            AND r.role_name IN ('Admin', 'Operations_Lead')
        )
    );

-- 4. Sample Policy: Groups Isolation (Coach sees only their assigned groups)
CREATE POLICY "Coaches view and manage their own groups" ON groups
    FOR ALL
    USING (
        auth.uid() = coach_id OR 
        EXISTS (
            SELECT 1 FROM user_roles ur
            JOIN roles r ON ur.role_id = r.role_id
            WHERE ur.user_id = auth.uid() 
            AND r.role_name IN ('Admin', 'Operations_Lead')
        )
    );

    -- 1. הגדרת פונקציית הטריגר לסנכרון נתונים מסכמת auth לסכמת public ושיוך תפקיד
CREATE OR REPLACE FUNCTION public.handle_new_user()
RETURNS TRIGGER
LANGUAGE plpgsql
SECURITY DEFINER SET search_path = public
AS $$
BEGIN
  -- א. אכלוס ישות המשתמש הבסיסית
  INSERT INTO public.users (user_id, email)
  VALUES (NEW.id, NEW.email);

  -- ב. שיוך אוטומטי של תפקיד ברירת מחדל 'מאמן' (Role ID = 3) בטבלת הקשר
  INSERT INTO public.user_roles (user_id, role_id)
  VALUES (NEW.id, 3);

  RETURN NEW;
END;
$$;

-- 2. קישור הפונקציה לאירוע הרשמה (After Insert)
CREATE OR REPLACE TRIGGER on_auth_user_created
  AFTER INSERT ON auth.users
  FOR EACH ROW EXECUTE PROCEDURE public.handle_new_user();


--- סוף סקריפט ה-SQL ---
