-- ========================================
-- Generalize document_type_configs -> type_configs
-- Add ResourceCategory rows for TrainingHub "skill category" lookup
-- ========================================

-- Renaming a table does not detach existing RLS policies, indexes or constraints
-- in PostgreSQL — they stay attached to the table's OID, not its name, so no
-- policy recreation is required.
ALTER TABLE document_type_configs RENAME TO type_configs;

-- Seed resource categories (idempotent) used by Admin > Training Hub > Add Resource.
-- category = 'ResourceCategory', doc_type = English code (used as TrainingResource.SkillCategory value
-- and as the storage folder segment for uploaded attachments).
INSERT INTO type_configs (category, doc_type, display_name, is_mandatory, requires_expiration, sort_order)
VALUES
	('ResourceCategory', 'Sports',            'ספורט',            false, false, 0),
	('ResourceCategory', 'Robotics',          'רובוטיקה',         false, false, 1),
	('ResourceCategory', 'HighSchool',        'תיכון',            false, false, 2),
	('ResourceCategory', 'SpecialEducation',  'חינוך מיוחד',      false, false, 3)
ON CONFLICT (category, doc_type) DO NOTHING;
