-- ==========================================
-- Migration: Allow coaches to read engagement name of their own contracts
-- Date: 2025
-- Purpose: Fix bug where coach-facing screens (assignments, calendar, documents,
--          groups, time report) always show "כללי" instead of the real
--          engagement name, because client_contracts had no RLS policy
--          granting coaches SELECT access at all (only Admin had access).
--
-- Scope: Coaches can only see rows of client_contracts that are linked to
--        one of their own assignments (via assignments.contract_id).
--        This still exposes billing_rate_per_hour/payment_terms columns to the
--        coach for their own engagements only (not for other coaches/clients),
--        which is an acceptable trade-off since RLS is row-level, not
--        column-level. If stricter column-level hiding is required, expose a
--        dedicated view instead.
-- ==========================================

DROP POLICY IF EXISTS "Coach read own contract engagement" ON public.client_contracts;

CREATE POLICY "Coach read own contract engagement" ON public.client_contracts
FOR SELECT TO authenticated
USING (
  contract_id IN (
	SELECT contract_id FROM public.assignments WHERE coach_id = auth.uid()
  )
);
