-- ========================================
-- הוספת סטטוס "Rejected" ל-time_entry_status_enum
-- Coach Manager — Time Entry Status Enhancement (v3.1)
-- ========================================

-- PostgreSQL doesn't allow direct modification of ENUMs, so we need to:
-- 1. Create new ENUM type with the new value
-- 2. Create a CAST from old to new type
-- 3. Alter the column to use the new type
-- 4. Drop the old type

-- Create the new ENUM type with all values including 'Rejected'
CREATE TYPE time_entry_status_enum_new AS ENUM ('Pending_Signature', 'Client_Approved', 'Admin_Approved', 'Billed_Paid', 'Rejected');

-- Alter the time_entries table to use the new ENUM type
-- Cast old values to new type text first, then to new ENUM
ALTER TABLE time_entries
	ALTER COLUMN status DROP DEFAULT,
	ALTER COLUMN status TYPE time_entry_status_enum_new USING status::text::time_entry_status_enum_new,
	ALTER COLUMN status SET DEFAULT 'Pending_Signature'::time_entry_status_enum_new;

-- Drop the old ENUM type
DROP TYPE time_entry_status_enum;

-- Rename the new type to the original name
ALTER TYPE time_entry_status_enum_new RENAME TO time_entry_status_enum;
