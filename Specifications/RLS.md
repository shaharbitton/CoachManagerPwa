-- ==========================================
-- 1. הפעלת מנגנון RLS על כל טבלאות הליבה במערכת
-- ==========================================
ALTER TABLE public.users ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.user_roles ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.coaches ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.coach_bank_details ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.clients ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.client_contracts ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.assignments ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.groups ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.students ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.time_entries ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.training_resources ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.coach_evaluations ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.coach_rates ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.coach_documents ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.signed_documents ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.coach_attributes ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.student_notes ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.roles ENABLE ROW LEVEL SECURITY;

-- ==========================================
-- 2. פונקציות עזר מאובטחות (SECURITY DEFINER)
-- (רצות בעוצמת עוקף-RLS כדי למנוע לולאות אינסופיות בבדיקות הרשאה)
-- ==========================================

CREATE OR REPLACE FUNCTION public.is_admin()
RETURNS BOOLEAN
LANGUAGE sql
SECURITY DEFINER
STABLE
AS $$
  SELECT EXISTS (
    SELECT 1 FROM public.user_roles 
    WHERE user_id = auth.uid() AND role_id = 1
  );
$$;

CREATE OR REPLACE FUNCTION public.is_coach()
RETURNS BOOLEAN
LANGUAGE sql
SECURITY DEFINER
STABLE
AS $$
  SELECT EXISTS (
    SELECT 1 FROM public.user_roles 
    WHERE user_id = auth.uid() AND role_id = 3
  );
$$;

-- ==========================================
-- 3. מחיקת כל הפוליסות הקיימות (Idempotent Reset)
-- ==========================================
DO $$
DECLARE
  _tbl TEXT;
  _pol RECORD;
BEGIN
  FOR _tbl IN
    SELECT unnest(ARRAY[
      'users','user_roles','coaches','coach_bank_details','clients',
      'client_contracts','assignments','groups','students','time_entries',
      'training_resources','coach_evaluations','coach_rates','coach_documents',
      'signed_documents','coach_attributes','student_notes','roles'
    ])
  LOOP
    FOR _pol IN
      SELECT policyname FROM pg_policies WHERE schemaname = 'public' AND tablename = _tbl
    LOOP
      EXECUTE format('DROP POLICY IF EXISTS %I ON public.%I', _pol.policyname, _tbl);
    END LOOP;
  END LOOP;
END;
$$;

-- ==========================================
-- 4. הרשאות מנהל מערכת (Admin Master Access)
-- ==========================================
CREATE POLICY "Admin full access" ON public.users FOR ALL TO authenticated USING (public.is_admin());
CREATE POLICY "Admin full access" ON public.user_roles FOR ALL TO authenticated USING (public.is_admin());
CREATE POLICY "Admin full access" ON public.coaches FOR ALL TO authenticated USING (public.is_admin());
CREATE POLICY "Admin full access" ON public.coach_bank_details FOR ALL TO authenticated USING (public.is_admin());
CREATE POLICY "Admin full access" ON public.clients FOR ALL TO authenticated USING (public.is_admin());
CREATE POLICY "Admin full access" ON public.client_contracts FOR ALL TO authenticated USING (public.is_admin());
CREATE POLICY "Admin full access" ON public.assignments FOR ALL TO authenticated USING (public.is_admin());
CREATE POLICY "Admin full access" ON public.groups FOR ALL TO authenticated USING (public.is_admin());
CREATE POLICY "Admin full access" ON public.students FOR ALL TO authenticated USING (public.is_admin());
CREATE POLICY "Admin full access" ON public.time_entries FOR ALL TO authenticated USING (public.is_admin());
CREATE POLICY "Admin full access" ON public.training_resources FOR ALL TO authenticated USING (public.is_admin());
CREATE POLICY "Admin full access" ON public.coach_evaluations FOR ALL TO authenticated USING (public.is_admin());
CREATE POLICY "Admin full access" ON public.coach_rates FOR ALL TO authenticated USING (public.is_admin());
CREATE POLICY "Admin full access" ON public.coach_documents FOR ALL TO authenticated USING (public.is_admin());
CREATE POLICY "Admin full access" ON public.signed_documents FOR ALL TO authenticated USING (public.is_admin());
CREATE POLICY "Admin full access" ON public.coach_attributes FOR ALL TO authenticated USING (public.is_admin());
CREATE POLICY "Admin full access" ON public.student_notes FOR ALL TO authenticated USING (public.is_admin());
CREATE POLICY "Admin full access" ON public.roles FOR ALL TO authenticated USING (public.is_admin());

-- ==========================================
-- 5. הרשאות מאמנים בשטח (Coaches Granular Access)
-- ==========================================

-- א. משתמשים ותפקידים
CREATE POLICY "All authenticated read roles" ON public.roles
FOR SELECT TO authenticated
USING (true);

CREATE POLICY "Coach read own user" ON public.users
FOR SELECT TO authenticated
USING (user_id = auth.uid());

CREATE POLICY "Coach read own user_role" ON public.user_roles
FOR SELECT TO authenticated
USING (user_id = auth.uid());

-- ב. פרופיל מאמן ופרטי בנק
CREATE POLICY "Coach read/update own profile" ON public.coaches
FOR ALL TO authenticated
USING (coach_id = auth.uid())
WITH CHECK (coach_id = auth.uid());

CREATE POLICY "Coach read/update own bank details" ON public.coach_bank_details
FOR ALL TO authenticated
USING (coach_id = auth.uid())
WITH CHECK (coach_id = auth.uid());

-- ג. שיבוצים ולקוחות
CREATE POLICY "Coach read own assignments" ON public.assignments
FOR SELECT TO authenticated
USING (coach_id = auth.uid());

CREATE POLICY "Coach read assigned clients" ON public.clients
FOR SELECT TO authenticated
USING (
  client_id IN (
    SELECT client_id FROM public.assignments WHERE coach_id = auth.uid()
  )
);

-- ד. קבוצות ותלמידים
CREATE POLICY "Coach manage own groups" ON public.groups
FOR ALL TO authenticated
USING (coach_id = auth.uid())
WITH CHECK (coach_id = auth.uid());

CREATE POLICY "Coach manage students in own groups" ON public.students
FOR ALL TO authenticated
USING (
  group_id IN (
    SELECT group_id FROM public.groups WHERE coach_id = auth.uid()
  )
)
WITH CHECK (
  group_id IN (
    SELECT group_id FROM public.groups WHERE coach_id = auth.uid()
  )
);

-- ה. דיווחי שעות
CREATE POLICY "Coach manage own time entries" ON public.time_entries
FOR ALL TO authenticated
USING (
  assign_id IN (
    SELECT assign_id FROM public.assignments WHERE coach_id = auth.uid()
  )
)
WITH CHECK (
  assign_id IN (
    SELECT assign_id FROM public.assignments WHERE coach_id = auth.uid()
  )
);

-- ו. מאגר הידע
CREATE POLICY "Coaches read resources" ON public.training_resources
FOR SELECT TO authenticated
USING (public.is_coach());

-- ז. תעריפי שכר
CREATE POLICY "Coach read own rates" ON public.coach_rates
FOR SELECT TO authenticated
USING (coach_id = auth.uid());

-- ח. מסמכים מנהליים
CREATE POLICY "Coach manage own documents" ON public.coach_documents
FOR ALL TO authenticated
USING (coach_id = auth.uid())
WITH CHECK (coach_id = auth.uid());

-- ט. חוזים חתומים
CREATE POLICY "Coach read own signed docs" ON public.signed_documents
FOR SELECT TO authenticated
USING (coach_id = auth.uid());

-- י. הערות תלמידים
CREATE POLICY "Coach manage own student notes" ON public.student_notes
FOR ALL TO authenticated
USING (coach_id = auth.uid())
WITH CHECK (coach_id = auth.uid());

-- ==========================================
-- 6. חסימות קשיחות (Explicit Denials by Omission)
-- ==========================================
-- שים לב: לא נוצרו פוליסות עבור מאמנים בטבלאות הבאות, ולכן הגישה אליהן חסומה להם לחלוטין ברמת המסד:
-- 1. client_contracts - הסכמי החיוב והרווח מול בתי הספר/עמותות מוסתרים.
-- 2. coach_evaluations - תיק ההערכות וה-HR הסודי גלוי לאדמין בלבד.
-- 3. coach_attributes - תגיות אופי ומיומנויות רכות גלויות לאדמין בלבד.

-- ==========================================
-- 7. Supabase Storage – מדיניות RLS עבור bucket "signatures"
-- ==========================================
-- יש להריץ ב-SQL Editor בדשבורד Supabase:

INSERT INTO storage.buckets (id, name, public)
VALUES ('signatures', 'signatures', true)
ON CONFLICT (id) DO NOTHING;

DROP POLICY IF EXISTS "Allow authenticated uploads to signatures" ON storage.objects;
CREATE POLICY "Allow authenticated uploads to signatures"
ON storage.objects FOR INSERT
TO authenticated
WITH CHECK (bucket_id = 'signatures');

DROP POLICY IF EXISTS "Allow authenticated read from signatures" ON storage.objects;
CREATE POLICY "Allow authenticated read from signatures"
ON storage.objects FOR SELECT
TO authenticated
USING (bucket_id = 'signatures');

DROP POLICY IF EXISTS "Allow public read from signatures" ON storage.objects;
CREATE POLICY "Allow public read from signatures"
ON storage.objects FOR SELECT
TO anon
USING (bucket_id = 'signatures');

-- ==========================================
-- 8. Supabase Storage – מדיניות RLS עבור bucket "documents"
-- ==========================================
-- מסמכים מאוחסנים תחת נתיב: {coach_id}/filename
-- מאמן רואה רק את הקבצים שלו, אדמין רואה הכל

INSERT INTO storage.buckets (id, name, public)
VALUES ('documents', 'documents', false)
ON CONFLICT (id) DO UPDATE SET public = false;

DROP POLICY IF EXISTS "Admin full access to documents" ON storage.objects;
CREATE POLICY "Admin full access to documents"
ON storage.objects FOR ALL
TO authenticated
USING (bucket_id = 'documents' AND public.is_admin())
WITH CHECK (bucket_id = 'documents' AND public.is_admin());

DROP POLICY IF EXISTS "Coach upload own documents" ON storage.objects;
CREATE POLICY "Coach upload own documents"
ON storage.objects FOR INSERT
TO authenticated
WITH CHECK (
  bucket_id = 'documents'
  AND (storage.foldername(name))[1] = auth.uid()::text
);

DROP POLICY IF EXISTS "Coach read own documents" ON storage.objects;
CREATE POLICY "Coach read own documents"
ON storage.objects FOR SELECT
TO authenticated
USING (
  bucket_id = 'documents'
  AND (storage.foldername(name))[1] = auth.uid()::text
);

-- ==========================================
-- 9. Supabase Storage – bucket "resources" (ציבורי)
-- ==========================================

INSERT INTO storage.buckets (id, name, public)
VALUES ('resources', 'resources', true)
ON CONFLICT (id) DO NOTHING;

DROP POLICY IF EXISTS "Allow authenticated uploads to resources" ON storage.objects;
CREATE POLICY "Allow authenticated uploads to resources"
ON storage.objects FOR INSERT
TO authenticated
WITH CHECK (bucket_id = 'resources');

DROP POLICY IF EXISTS "Allow public read from resources" ON storage.objects;
CREATE POLICY "Allow public read from resources"
ON storage.objects FOR SELECT
TO anon
USING (bucket_id = 'resources');

DROP POLICY IF EXISTS "Allow authenticated read from resources" ON storage.objects;
CREATE POLICY "Allow authenticated read from resources"
ON storage.objects FOR SELECT
TO authenticated
USING (bucket_id = 'resources');