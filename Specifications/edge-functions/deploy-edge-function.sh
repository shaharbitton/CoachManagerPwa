#!/bin/bash
# ==============================================
# סקריפט פריסת Edge Function + הגדרת Secrets
# ⚠️ סקריפט זה דורש Supabase CLI — לא ניתן להריץ ב-SQL Editor
# לגרסת SQL ראו: deploy-edge-function.sql
# ==============================================
# דרישות מקדימות:
#   1. התקנת Supabase CLI: npm install -g supabase
#   2. התחברות: supabase login
#   3. קישור לפרויקט: supabase link --project-ref <PROJECT_REF>
#
# לפרויקט Dev:  PROJECT_REF = spyalzbjcfdrkbyqkopa
# לפרויקט Prod: PROJECT_REF = wwzrsibzpjlckjocjfpn
# ==============================================

# --- שלב 1: הגדרת Secrets ---
echo "🔐 הגדרת Edge Function Secrets..."

supabase secrets set BREVO_API_KEY="xkeysib-YOUR-BREVO-API-KEY-HERE"
supabase secrets set BREVO_FROM_EMAIL="donot_replay_arcan@mop.co.il"
supabase secrets set BREVO_FROM_NAME="Arcan Israel"

echo "✅ Secrets הוגדרו בהצלחה"

# --- שלב 2: יצירת תיקיית Edge Function ---
echo "📁 יצירת תיקיית Edge Function..."

mkdir -p supabase/functions/send-welcome-email

# --- שלב 3: יצירת קובץ הפונקציה ---
cat > supabase/functions/send-welcome-email/index.ts << 'EOF'
// Supabase Edge Function: send-welcome-email
// Deploy: supabase functions deploy send-welcome-email

import { serve } from "https://deno.land/std@0.177.0/http/server.ts";

const corsHeaders = {
  "Access-Control-Allow-Origin": "*",
  "Access-Control-Allow-Headers": "authorization, x-client-info, apikey, content-type",
};

serve(async (req: Request) => {
  // Handle CORS preflight
  if (req.method === "OPTIONS") {
    return new Response("ok", { headers: corsHeaders });
  }

  try {
    const { toEmail, coachFirstName } = await req.json();

    if (!toEmail || !coachFirstName) {
      return new Response(
        JSON.stringify({ error: "Missing toEmail or coachFirstName" }),
        { status: 400, headers: { ...corsHeaders, "Content-Type": "application/json" } }
      );
    }

    const apiKey = Deno.env.get("BREVO_API_KEY");
    const fromEmail = Deno.env.get("BREVO_FROM_EMAIL") || "donot_replay_arcan@mop.co.il";
    const fromName = Deno.env.get("BREVO_FROM_NAME") || "Arcan Israel";

    if (!apiKey) {
      return new Response(
        JSON.stringify({ error: "BREVO_API_KEY not configured" }),
        { status: 500, headers: { ...corsHeaders, "Content-Type": "application/json" } }
      );
    }

    const htmlBody = `
      <div dir="rtl" style="font-family: Arial, sans-serif; line-height: 1.8; color: #333;">
        <div style="max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 8px;">
          <div style="text-align: center; margin-bottom: 20px;">
            <img src="https://shaharbitton.github.io/CoachManagerPwa/icon-512.png" alt="Arcan Israel" width="80" height="80" style="display: block; margin: 0 auto 8px auto; border-radius: 50%;" />
            <h2 style="color: #1a365d; margin: 0;">Arcan Israel</h2>
            <p style="color: #666; margin: 4px 0;">מערכת ניהול מאמנים ושטח</p>
          </div>
          <hr style="border: none; border-top: 2px solid #1a365d; margin: 16px 0;" />
          <p>שלום <strong>${coachFirstName}</strong>,</p>
          <p>נוצר עבורך חשבון במערכת ניהול המאמנים של Arcan Israel.</p>
          <div style="background-color: #f7fafc; border: 1px solid #e2e8f0; border-radius: 6px; padding: 16px; margin: 16px 0;">
            <p style="margin: 4px 0;">🔗 <a href="https://shaharbitton.github.io/CoachManagerPwa/" style="color: #1a365d; font-weight: bold;">לכניסה למערכת</a></p>
            <p style="margin: 4px 0;">📧 שם משתמש: <strong>${toEmail}</strong></p>
            <p style="margin: 4px 0;">🔑 סיסמה ראשונית: <strong>Coach123!</strong></p>
          </div>
          <p>בכניסה הראשונה תתבקש/י לשנות סיסמה, להשלים פרטים אישיים ולהעלות אישורים כגון אישור משטרה.</p>
          <p>בהצלחה!<br/><strong>צוות Arcan Israel</strong></p>
        </div>
      </div>`;

    const brevoResponse = await fetch("https://api.brevo.com/v3/smtp/email", {
      method: "POST",
      headers: {
        "api-key": apiKey,
        "Content-Type": "application/json",
      },
      body: JSON.stringify({
        sender: { email: fromEmail, name: fromName },
        to: [{ email: toEmail, name: coachFirstName }],
        subject: "ברוך הבא למערכת Arcan Israel",
        htmlContent: htmlBody,
      }),
    });

    if (!brevoResponse.ok) {
      const errorText = await brevoResponse.text();
      return new Response(
        JSON.stringify({ error: `Brevo API error: ${errorText}` }),
        { status: brevoResponse.status, headers: { ...corsHeaders, "Content-Type": "application/json" } }
      );
    }

    return new Response(
      JSON.stringify({ success: true }),
      { headers: { ...corsHeaders, "Content-Type": "application/json" } }
    );
  } catch (error) {
    return new Response(
      JSON.stringify({ error: error.message }),
      { status: 500, headers: { ...corsHeaders, "Content-Type": "application/json" } }
    );
  }
});
EOF

echo "✅ קובץ הפונקציה נוצר"

# --- שלב 3ב: יצירת פונקציית התראה על חוזה לחתימה ---
echo "📁 יצירת תיקיית send-contract-notification..."

mkdir -p supabase/functions/send-contract-notification

cat > supabase/functions/send-contract-notification/index.ts << 'EOF'
// Supabase Edge Function: send-contract-notification
// Deploy: supabase functions deploy send-contract-notification
// מקור מלא: Specifications/edge-functions/send-contract-notification.txt

import { serve } from "https://deno.land/std@0.177.0/http/server.ts";

const corsHeaders = {
  "Access-Control-Allow-Origin": "*",
  "Access-Control-Allow-Headers": "authorization, x-client-info, apikey, content-type",
};

serve(async (req: Request) => {
  if (req.method === "OPTIONS") {
    return new Response("ok", { headers: corsHeaders });
  }

  try {
    const { toEmail, coachFirstName, isReminder } = await req.json();

    if (!toEmail || !coachFirstName) {
      return new Response(
        JSON.stringify({ error: "Missing toEmail or coachFirstName" }),
        { status: 400, headers: { ...corsHeaders, "Content-Type": "application/json" } }
      );
    }

    const apiKey = Deno.env.get("BREVO_API_KEY");
    const fromEmail = Deno.env.get("BREVO_FROM_EMAIL") || "donot_replay_arcan@mop.co.il";
    const fromName = Deno.env.get("BREVO_FROM_NAME") || "Arcan Israel";

    if (!apiKey) {
      return new Response(
        JSON.stringify({ error: "BREVO_API_KEY not configured" }),
        { status: 500, headers: { ...corsHeaders, "Content-Type": "application/json" } }
      );
    }

    const title = isReminder ? "תזכורת: חוזה ממתין לחתימתך" : "חוזה חדש ממתין לחתימתך";
    const intro = isReminder
      ? `זוהי תזכורת — עדיין ממתין לך חוזה לחתימה במערכת ניהול המאמנים של Arcan Israel.`
      : `ממתין לך חוזה חדש לחתימה במערכת ניהול המאמנים של Arcan Israel.`;

    const htmlBody = `
      <div dir="rtl" style="font-family: Arial, sans-serif; line-height: 1.8; color: #333;">
        <div style="max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 8px;">
          <div style="text-align: center; margin-bottom: 20px;">
            <img src="https://shaharbitton.github.io/CoachManagerPwa/icon-512.png" alt="Arcan Israel" width="80" height="80" style="display: block; margin: 0 auto 8px auto; border-radius: 50%;" />
            <h2 style="color: #1a365d; margin: 0;">Arcan Israel</h2>
            <p style="color: #666; margin: 4px 0;">מערכת ניהול מאמנים ושטח</p>
          </div>
          <hr style="border: none; border-top: 2px solid #1a365d; margin: 16px 0;" />
          <p>שלום <strong>${coachFirstName}</strong>,</p>
          <p>${intro}</p>
          <div style="background-color: #f7fafc; border: 1px solid #e2e8f0; border-radius: 6px; padding: 16px; margin: 16px 0;">
            <p style="margin: 4px 0;">כדי לצפות בחוזה ולחתום עליו:</p>
            <ol style="margin: 8px 0; padding-right: 20px;">
              <li><a href="https://shaharbitton.github.io/CoachManagerPwa/" style="color: #1a365d; font-weight: bold;">היכנס/י למערכת</a></li>
              <li>עבור/י לתפריט <strong>"מסמכים"</strong></li>
              <li>לחץ/י על החוזה הממתין ובצע/י חתימה דיגיטלית</li>
            </ol>
          </div>
          <p>לאחר החתימה החוזה יישמר במערכת ותקבל/י עותק חתום.</p>
          <p>תודה,<br/><strong>צוות Arcan Israel</strong></p>
        </div>
      </div>`;

    const brevoResponse = await fetch("https://api.brevo.com/v3/smtp/email", {
      method: "POST",
      headers: {
        "api-key": apiKey,
        "Content-Type": "application/json",
      },
      body: JSON.stringify({
        sender: { email: fromEmail, name: fromName },
        to: [{ email: toEmail, name: coachFirstName }],
        subject: title,
        htmlContent: htmlBody,
      }),
    });

    if (!brevoResponse.ok) {
      const errorText = await brevoResponse.text();
      return new Response(
        JSON.stringify({ error: `Brevo API error: ${errorText}` }),
        { status: brevoResponse.status, headers: { ...corsHeaders, "Content-Type": "application/json" } }
      );
    }

    return new Response(
      JSON.stringify({ success: true }),
      { headers: { ...corsHeaders, "Content-Type": "application/json" } }
    );
  } catch (error) {
    return new Response(
      JSON.stringify({ error: error.message }),
      { status: 500, headers: { ...corsHeaders, "Content-Type": "application/json" } }
    );
  }
});
EOF

echo "✅ קובץ הפונקציה send-contract-notification נוצר"

# --- שלב 3ג: יצירת פונקציית תזכורת להעלאת מסמך ---
echo "📁 יצירת תיקיית send-document-reminder..."

mkdir -p supabase/functions/send-document-reminder

cat > supabase/functions/send-document-reminder/index.ts << 'EOF'
// Supabase Edge Function: send-document-reminder
// Deploy: supabase functions deploy send-document-reminder
// מקור מלא: Specifications/edge-functions/send-document-reminder.txt

import { serve } from "https://deno.land/std@0.177.0/http/server.ts";

const corsHeaders = {
  "Access-Control-Allow-Origin": "*",
  "Access-Control-Allow-Headers": "authorization, x-client-info, apikey, content-type",
};

serve(async (req: Request) => {
  if (req.method === "OPTIONS") {
    return new Response("ok", { headers: corsHeaders });
  }

  try {
    const { toEmail, coachFirstName, documentName } = await req.json();

    if (!toEmail || !coachFirstName || !documentName) {
      return new Response(
        JSON.stringify({ error: "Missing toEmail, coachFirstName or documentName" }),
        { status: 400, headers: { ...corsHeaders, "Content-Type": "application/json" } }
      );
    }

    const apiKey = Deno.env.get("BREVO_API_KEY");
    const fromEmail = Deno.env.get("BREVO_FROM_EMAIL") || "donot_replay_arcan@mop.co.il";
    const fromName = Deno.env.get("BREVO_FROM_NAME") || "Arcan Israel";

    if (!apiKey) {
      return new Response(
        JSON.stringify({ error: "BREVO_API_KEY not configured" }),
        { status: 500, headers: { ...corsHeaders, "Content-Type": "application/json" } }
      );
    }

    const htmlBody = `
      <div dir="rtl" style="font-family: Arial, sans-serif; line-height: 1.8; color: #333;">
        <div style="max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 8px;">
          <div style="text-align: center; margin-bottom: 20px;">
            <img src="https://shaharbitton.github.io/CoachManagerPwa/icon-512.png" alt="Arcan Israel" width="80" height="80" style="display: block; margin: 0 auto 8px auto; border-radius: 50%;" />
            <h2 style="color: #1a365d; margin: 0;">Arcan Israel</h2>
            <p style="color: #666; margin: 4px 0;">מערכת ניהול מאמנים ושטח</p>
          </div>
          <hr style="border: none; border-top: 2px solid #1a365d; margin: 16px 0;" />
          <p>שלום <strong>${coachFirstName}</strong>,</p>
          <p>לתשומת ליבך — טרם הועלה למערכת המסמך הבא: <strong>${documentName}</strong>.</p>
          <div style="background-color: #f7fafc; border: 1px solid #e2e8f0; border-radius: 6px; padding: 16px; margin: 16px 0;">
            <p style="margin: 4px 0;">כדי להעלות את המסמך:</p>
            <ol style="margin: 8px 0; padding-right: 20px;">
              <li><a href="https://shaharbitton.github.io/CoachManagerPwa/" style="color: #1a365d; font-weight: bold;">היכנס/י למערכת</a></li>
              <li>עבור/י לתפריט <strong>"מסמכים"</strong></li>
              <li>לחץ/י על "העלאת מסמך חדש" ובחר/י את סוג המסמך המבוקש</li>
            </ol>
          </div>
          <p>תודה על שיתוף הפעולה,<br/><strong>צוות Arcan Israel</strong></p>
        </div>
      </div>`;

    const brevoResponse = await fetch("https://api.brevo.com/v3/smtp/email", {
      method: "POST",
      headers: {
        "api-key": apiKey,
        "Content-Type": "application/json",
      },
      body: JSON.stringify({
        sender: { email: fromEmail, name: fromName },
        to: [{ email: toEmail, name: coachFirstName }],
        subject: `תזכורת: נדרש להעלות ${documentName}`,
        htmlContent: htmlBody,
      }),
    });

    if (!brevoResponse.ok) {
      const errorText = await brevoResponse.text();
      return new Response(
        JSON.stringify({ error: `Brevo API error: ${errorText}` }),
        { status: brevoResponse.status, headers: { ...corsHeaders, "Content-Type": "application/json" } }
      );
    }

    return new Response(
      JSON.stringify({ success: true }),
      { headers: { ...corsHeaders, "Content-Type": "application/json" } }
    );
  } catch (error) {
    return new Response(
      JSON.stringify({ error: error.message }),
      { status: 500, headers: { ...corsHeaders, "Content-Type": "application/json" } }
    );
  }
});
EOF

echo "✅ קובץ הפונקציה send-document-reminder נוצר"

# --- שלב 4: פריסה ---
echo "🚀 פריסת Edge Functions..."

supabase functions deploy send-welcome-email --no-verify-jwt
supabase functions deploy send-contract-notification --no-verify-jwt
supabase functions deploy send-document-reminder --no-verify-jwt

echo "✅ Edge Functions נפרסו בהצלחה!"

# --- שלב 5: אימות ---
echo ""
echo "📋 לאימות הגדרות:"
echo "   supabase secrets list"
echo ""
echo "📋 לבדיקת הפונקציות:"
echo "   curl -X POST '<SUPABASE_URL>/functions/v1/send-welcome-email' \\"
echo "     -H 'Authorization: Bearer <ANON_KEY>' \\"
echo "     -H 'Content-Type: application/json' \\"
echo "     -d '{\"toEmail\": \"test@example.com\", \"coachFirstName\": \"בדיקה\"}'"
echo ""
echo "   curl -X POST '<SUPABASE_URL>/functions/v1/send-contract-notification' \\"
echo "     -H 'Authorization: Bearer <ANON_KEY>' \\"
echo "     -H 'Content-Type: application/json' \\"
echo "     -d '{\"toEmail\": \"test@example.com\", \"coachFirstName\": \"בדיקה\", \"isReminder\": false}'"
echo ""
echo "   curl -X POST '<SUPABASE_URL>/functions/v1/send-document-reminder' \\"
echo "     -H 'Authorization: Bearer <ANON_KEY>' \\"
echo "     -H 'Content-Type: application/json' \\"
echo "     -d '{\"toEmail\": \"test@example.com\", \"coachFirstName\": \"בדיקה\", \"documentName\": \"אישור משטרה\"}'"
