// Supabase Edge Function: reset-coach-password
// Resets a coach's password to "Coach123!" and sends notification email via Brevo
// Deploy: supabase functions deploy reset-coach-password --no-verify-jwt

import { serve } from "https://deno.land/std@0.177.0/http/server.ts";
import { createClient } from "https://esm.sh/@supabase/supabase-js@2";

const corsHeaders = {
  "Access-Control-Allow-Origin": "*",
  "Access-Control-Allow-Headers": "authorization, x-client-info, apikey, content-type",
};

serve(async (req: Request) => {
  if (req.method === "OPTIONS") {
    return new Response("ok", { headers: corsHeaders });
  }

  try {
    const { userId, toEmail, coachFirstName } = await req.json();

    if (!userId || !toEmail || !coachFirstName) {
      return new Response(
        JSON.stringify({ error: "Missing userId, toEmail, or coachFirstName" }),
        { status: 400, headers: { ...corsHeaders, "Content-Type": "application/json" } }
      );
    }

    // Use service_role key to update auth user password
    const supabaseUrl = Deno.env.get("SUPABASE_URL")!;
    const serviceRoleKey = Deno.env.get("SUPABASE_SERVICE_ROLE_KEY")!;
    const supabase = createClient(supabaseUrl, serviceRoleKey);

    const defaultPassword = "Coach123!";

    const { error: updateError } = await supabase.auth.admin.updateUserById(userId, {
      password: defaultPassword,
    });

    if (updateError) {
      return new Response(
        JSON.stringify({ error: `Failed to reset password: ${updateError.message}` }),
        { status: 500, headers: { ...corsHeaders, "Content-Type": "application/json" } }
      );
    }

    // Send notification email via Brevo
    const apiKey = Deno.env.get("BREVO_API_KEY");
    const fromEmail = Deno.env.get("BREVO_FROM_EMAIL") || "donot_replay_arcan@mop.co.il";
    const fromName = Deno.env.get("BREVO_FROM_NAME") || "Arcan Israel";

    if (apiKey) {
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
            <p>הסיסמה שלך במערכת אופסה על ידי מנהל המערכת.</p>
            <div style="background-color: #fff3cd; border: 1px solid #ffc107; border-radius: 6px; padding: 16px; margin: 16px 0;">
              <p style="margin: 4px 0;">🔗 <a href="https://shaharbitton.github.io/CoachManagerPwa/" style="color: #1a365d; font-weight: bold;">לכניסה למערכת</a></p>
              <p style="margin: 4px 0;">📧 שם משתמש: <strong>${toEmail}</strong></p>
              <p style="margin: 4px 0;">🔑 סיסמה חדשה: <strong>${defaultPassword}</strong></p>
            </div>
            <p>בכניסה הבאה תתבקש/י לשנות את הסיסמה.</p>
            <p>בהצלחה!<br/><strong>צוות Arcan Israel</strong></p>
          </div>
        </div>`;

      await fetch("https://api.brevo.com/v3/smtp/email", {
        method: "POST",
        headers: {
          "api-key": apiKey,
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          sender: { email: fromEmail, name: fromName },
          to: [{ email: toEmail, name: coachFirstName }],
          subject: "איפוס סיסמה - Arcan Israel",
          htmlContent: htmlBody,
        }),
      });
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
