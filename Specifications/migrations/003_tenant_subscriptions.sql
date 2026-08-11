-- ========================================
-- יצירת טבלת tenant_subscriptions
-- Coach Manager — Feature Tiering (v3.0)
-- ========================================

CREATE TABLE IF NOT EXISTS tenant_subscriptions (
    subscription_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL REFERENCES auth.users(id),
    tier INT NOT NULL DEFAULT 0,           -- 0=Basic, 1=Pro, 2=Enterprise
    started_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    expires_at TIMESTAMPTZ,                -- NULL = לצמיתות
    is_active BOOLEAN NOT NULL DEFAULT true,
    payment_ref TEXT,                       -- מזהה תשלום חיצוני (Stripe/PayPal)
    created_at TIMESTAMPTZ NOT NULL DEFAULT now()
);

-- אינדקס לחיפוש מהיר לפי tenant
CREATE INDEX IF NOT EXISTS idx_tenant_subscriptions_tenant_id ON tenant_subscriptions(tenant_id);

-- RLS
ALTER TABLE tenant_subscriptions ENABLE ROW LEVEL SECURITY;

-- אדמין יכול לקרוא את כל המנויים
CREATE POLICY "admin_read_all_subscriptions" ON tenant_subscriptions
    FOR SELECT USING (true);

-- כל משתמש יכול לקרוא את המנוי שלו
CREATE POLICY "user_read_own_subscription" ON tenant_subscriptions
    FOR SELECT USING (auth.uid() = tenant_id);

-- ========================================
-- דוגמה: הכנסת מנוי Pro למשתמש אדמין
-- החלף את ה-UUID ב-user id של האדמין
-- ========================================
-- INSERT INTO tenant_subscriptions (tenant_id, tier, is_active)
-- VALUES ('YOUR-ADMIN-USER-ID-HERE', 1, true);
