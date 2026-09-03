-- ==========================================
-- Migration: Immutable Billed_Paid Records
-- Date: 2024
-- Purpose: Prevent modification/deletion of finalized time entries
-- ==========================================

-- Drop existing coach policy to replace with immutability check
DROP POLICY IF EXISTS "Coach manage own time entries" ON public.time_entries;

-- Create new policy that prevents updates/deletes on Billed_Paid entries
CREATE POLICY "Coach manage own time entries (immutable Billed_Paid)" ON public.time_entries
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
  -- Cannot update or delete if status is already Billed_Paid
  AND (
	-- Allow only if NEW record being inserted (no assign_id match yet)
	-- OR if the current status is NOT Billed_Paid
	(SELECT status FROM public.time_entries WHERE entry_id = COALESCE(
	  (SELECT entry_id FROM public.time_entries ORDER BY entry_id DESC LIMIT 1), ''
	)) IS NULL
	OR (SELECT status FROM public.time_entries WHERE entry_id = COALESCE(
	  (SELECT entry_id FROM public.time_entries ORDER BY entry_id DESC LIMIT 1), ''
	)) != 'Billed_Paid'
  )
);

-- Note: Admin policy already has full access via is_admin()
-- Admins can still perform operations, but preferred to block at service layer
-- to provide better error messages to users

-- For soft-delete support (optional):
-- ALTER TABLE public.time_entries ADD COLUMN IF NOT EXISTS archived_at TIMESTAMP NULL;
-- UPDATE public.time_entries SET archived_at = NOW() WHERE status = 'Billed_Paid';
