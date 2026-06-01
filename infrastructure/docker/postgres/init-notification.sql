CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

CREATE TABLE IF NOT EXISTS "EmailTemplates" (
    "Id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "Name" VARCHAR(128) NOT NULL UNIQUE,
    "Subject" VARCHAR(256) NOT NULL,
    "Body" TEXT NOT NULL,
    "IsHtml" BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMPTZ
);

CREATE TABLE IF NOT EXISTS "Notifications" (
    "Id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "UserId" UUID NOT NULL,
    "Type" VARCHAR(64) NOT NULL,
    "Title" VARCHAR(256) NOT NULL,
    "Message" TEXT NOT NULL,
    "IsRead" BOOLEAN NOT NULL DEFAULT FALSE,
    "Metadata" JSONB,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

INSERT INTO "EmailTemplates" ("Name", "Subject", "Body", "IsHtml")
VALUES
(
    'WelcomeEmail',
    'Welcome to DeskMatch!',
    '<h1>Welcome, {{FirstName}}!</h1><p>Thank you for joining DeskMatch. We are excited to have you on board.</p><p>Start exploring workspaces near you and book your first desk today.</p><p>Best regards,<br/>The DeskMatch Team</p>',
    TRUE
),
(
    'DepositPaymentConfirmation',
    'Deposit Payment Received — {{OfficeName}}',
    '<h1>Deposit Payment Confirmed</h1><p>Hi {{FirstName}},</p><p>Your deposit of <strong>${{DepositAmount}}</strong> for <strong>{{OfficeName}}</strong> has been received. Your reservation is now <strong>Confirmed</strong>.</p><p><strong>Remaining balance:</strong> ${{RemainingBalance}}<br/><strong>Reservation:</strong> {{StartTime}} — {{EndTime}}</p><p>The remaining balance must be paid before the reservation start time.</p><p>Best regards,<br/>The DeskMatch Team</p>',
    TRUE
),
(
    'FinalPaymentConfirmation',
    'Final Payment Received — Fully Paid',
    '<h1>Payment Complete</h1><p>Hi {{FirstName}},</p><p>Your final payment of <strong>${{FinalAmount}}</strong> for <strong>{{OfficeName}}</strong> has been received. Your reservation is now <strong>Fully Paid</strong>.</p><p><strong>Total paid:</strong> ${{TotalPaid}}<br/><strong>Reservation:</strong> {{StartTime}} — {{EndTime}}</p><p>We look forward to seeing you!</p><p>Best regards,<br/>The DeskMatch Team</p>',
    TRUE
),
(
    'PaymentReminder',
    'Reminder: Payment Due for {{OfficeName}}',
    '<h1>Payment Reminder</h1><p>Hi {{FirstName}},</p><p>This is a reminder that you have an outstanding balance of <strong>${{RemainingBalance}}</strong> for your reservation at <strong>{{OfficeName}}</strong>.</p><p><strong>Reservation:</strong> {{StartTime}} — {{EndTime}}</p><p>Please complete your payment before the reservation starts to avoid cancellation.</p><p><a href="{{PaymentUrl}}" style="background:#2563eb;color:white;padding:12px 24px;text-decoration:none;border-radius:6px">Pay Now</a></p><p>Best regards,<br/>The DeskMatch Team</p>',
    TRUE
),
(
    'ReservationConfirmation',
    'Your Reservation is Confirmed',
    '<h1>Reservation Confirmed</h1><p>Hi {{FirstName}},</p><p>Your reservation at <strong>{{OfficeName}}</strong> has been confirmed.</p><p><strong>Date:</strong> {{ReservationDate}}<br/><strong>Time:</strong> {{StartTime}} - {{EndTime}}<br/><strong>Address:</strong> {{OfficeAddress}}</p><p>We look forward to seeing you!</p><p>Best regards,<br/>The DeskMatch Team</p>',
    TRUE
),
(
    'ReservationCancelled',
    'Your Reservation Has Been Cancelled',
    '<h1>Reservation Cancelled</h1><p>Hi {{FirstName}},</p><p>Your reservation at <strong>{{OfficeName}}</strong> for {{ReservationDate}} has been cancelled.</p><p>If you did not request this cancellation, please contact our support team.</p><p>You can book another workspace at any time from your dashboard.</p><p>Best regards,<br/>The DeskMatch Team</p>',
    TRUE
)
ON CONFLICT ("Name") DO NOTHING;