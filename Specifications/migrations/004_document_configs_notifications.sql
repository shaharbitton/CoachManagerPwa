-- ========================================
-- Document type configuration + one-time notification acknowledgements
-- Coach Manager — Notification alignment (v4.0)
-- ========================================

-- ---- Document / contract type configuration (admin-managed) ----
CREATE TABLE IF NOT EXISTS document_type_configs (
    config_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    category TEXT NOT NULL DEFAULT 'Document',   -- 'Document' | 'Contract'
    doc_type TEXT NOT NULL,                       -- machine identifier, e.g. 'National_ID_Card'
    display_name TEXT NOT NULL,                    -- Hebrew display name
    is_mandatory BOOLEAN NOT NULL DEFAULT false,
    requires_expiration BOOLEAN NOT NULL DEFAULT false,
    is_active BOOLEAN NOT NULL DEFAULT true,
    sort_order INT NOT NULL DEFAULT 0,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    UNIQUE (category, doc_type)
);

ALTER TABLE document_type_configs ENABLE ROW LEVEL SECURITY;

-- Everyone authenticated may read the configuration (coaches need it for their notifications)
CREATE POLICY "read_document_type_configs" ON document_type_configs
    FOR SELECT USING (auth.role() = 'authenticated');

-- Only admins may modify. Adjust the admin check to your role model as needed.
CREATE POLICY "admin_write_document_type_configs" ON document_type_configs
    FOR ALL USING (
        EXISTS (
            SELECT 1 FROM user_roles ur
            JOIN roles r ON r.role_id = ur.role_id
            WHERE ur.user_id = auth.uid()
              AND r.role_name IN ('Admin', 'Operations_Lead')
        )
    );

-- ---- One-time notification acknowledgements (per coach) ----
CREATE TABLE IF NOT EXISTS notification_acknowledgements (
    ack_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    coach_id TEXT NOT NULL,
    notification_key TEXT NOT NULL,               -- stable event key, e.g. 'doc-rejected:{docId}'
    acknowledged_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    UNIQUE (coach_id, notification_key)
);

CREATE INDEX IF NOT EXISTS idx_notification_acks_coach ON notification_acknowledgements(coach_id);

ALTER TABLE notification_acknowledgements ENABLE ROW LEVEL SECURITY;

-- A coach may read and create only their own acknowledgements
CREATE POLICY "coach_read_own_acks" ON notification_acknowledgements
    FOR SELECT USING (auth.uid()::text = coach_id);

CREATE POLICY "coach_insert_own_acks" ON notification_acknowledgements
    FOR INSERT WITH CHECK (auth.uid()::text = coach_id);

-- ========================================
-- Seed default document types (idempotent)
-- ========================================
INSERT INTO document_type_configs (category, doc_type, display_name, is_mandatory, requires_expiration, sort_order)
VALUES
    ('Document', 'National_ID_Card',  'תעודת זהות',        true,  false, 0),
    ('Document', 'Police_Clearance',  'אישור משטרה',       true,  true,  1),
    ('Document', 'Certification',     'תעודת הסמכה',       true,  true,  2),
    ('Document', 'Tax_Withholding',   'ניכוי מס במקור',    true,  true,  3),
    ('Document', 'Bank_Confirmation', 'אישור ניהול חשבון', true,  false, 4),
    ('Document', 'Recommendations',   'המלצות',            false, false, 5)
ON CONFLICT (category, doc_type) DO NOTHING;
