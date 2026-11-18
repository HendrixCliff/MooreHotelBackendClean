START TRANSACTION;

ALTER TABLE "Payment" DROP CONSTRAINT "FK_Payment_Bookings_BookingId";

ALTER TABLE "Payment" DROP CONSTRAINT "PK_Payment";

ALTER TABLE "Payment" RENAME TO "Payments";

ALTER INDEX "IX_Payment_BookingId" RENAME TO "IX_Payments_BookingId";

ALTER TABLE "Payments" ADD "CreatedAt" timestamp with time zone NOT NULL DEFAULT TIMESTAMPTZ '-infinity';

ALTER TABLE "Payments" ADD CONSTRAINT "PK_Payments" PRIMARY KEY ("Id");

ALTER TABLE "Payments" ADD CONSTRAINT "FK_Payments_Bookings_BookingId" FOREIGN KEY ("BookingId") REFERENCES "Bookings" ("Id") ON DELETE CASCADE;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260220125615_AddPaymentsTable', '8.0.10');

COMMIT;

START TRANSACTION;

ALTER TABLE "Bookings" ADD "GroupReference" text NOT NULL DEFAULT '';

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260224094033_AddGroupReferenceToBookings', '8.0.10');

COMMIT;

START TRANSACTION;

ALTER TABLE "Bookings" ADD "UpdatedAt" timestamp with time zone NOT NULL DEFAULT TIMESTAMPTZ '-infinity';

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260227112621_AddUpdatedAtToBooking', '8.0.10');

COMMIT;

START TRANSACTION;

ALTER TABLE "Bookings" DROP CONSTRAINT "FK_Bookings_Rooms_RoomId";

ALTER TABLE "AspNetUsers" DROP COLUMN "CreatedByAdminId";

ALTER TABLE "ServiceOrders" ADD "TotalAmount" numeric NOT NULL DEFAULT 0.0;

ALTER TABLE "Payments" ALTER COLUMN "Amount" TYPE numeric(18,2);

ALTER TABLE "OrderItems" ADD "RoomNumber" text;

ALTER TABLE "AspNetUsers" ADD "IsActive" boolean NOT NULL DEFAULT FALSE;

CREATE INDEX "IX_ServiceOrders_GuestId" ON "ServiceOrders" ("GuestId");

ALTER TABLE "Bookings" ADD CONSTRAINT "FK_Bookings_Rooms_RoomId" FOREIGN KEY ("RoomId") REFERENCES "Rooms" ("Id") ON DELETE RESTRICT;

ALTER TABLE "ServiceOrders" ADD CONSTRAINT "FK_ServiceOrders_Guests_GuestId" FOREIGN KEY ("GuestId") REFERENCES "Guests" ("Id");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261111163620_AddEmployeeActiveStatus', '8.0.10');

COMMIT;

