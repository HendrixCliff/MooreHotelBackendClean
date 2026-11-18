using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MooreHotelAndSuites.Infrastructure.Persistence.Migrations;

public partial class AddEmployeeActiveStatus : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Preserve CreatedByAdminId: it is no longer used by the application,
        // but dropping it would permanently discard any historical values.

        // Ensure ServiceOrders.TotalAmount exists.
        migrationBuilder.Sql(
            """
            ALTER TABLE "ServiceOrders"
            ADD COLUMN IF NOT EXISTS "TotalAmount" numeric NOT NULL DEFAULT 0.0;
            """);

        // Add IsActive without overwriting existing account data.
        migrationBuilder.Sql(
            """
            ALTER TABLE "AspNetUsers"
            ADD COLUMN IF NOT EXISTS "IsActive" boolean NOT NULL DEFAULT TRUE;
            """);

        // Convert OrderItems.RoomNumber to text only if necessary.
        migrationBuilder.Sql(
            """
            DO $$
            DECLARE
                current_type text;
            BEGIN
                SELECT data_type
                INTO current_type
                FROM information_schema.columns
                WHERE table_schema = 'public'
                  AND table_name = 'OrderItems'
                  AND column_name = 'RoomNumber';

                IF current_type IS NOT NULL AND current_type <> 'text' THEN
                    ALTER TABLE "OrderItems"
                    ALTER COLUMN "RoomNumber" TYPE text
                    USING "RoomNumber"::text;
                END IF;
            END $$;
            """);

        // Convert Payments columns to match the current Payment entity.
        migrationBuilder.Sql(
            """
            DO $$
            DECLARE
                current_type text;
                invalid_count bigint;
            BEGIN
                -- Amount: numeric(18,2)
                SELECT data_type
                INTO current_type
                FROM information_schema.columns
                WHERE table_schema = 'public'
                  AND table_name = 'Payments'
                  AND column_name = 'Amount';

                IF current_type IS NOT NULL THEN
                    ALTER TABLE "Payments"
                    ALTER COLUMN "Amount" TYPE numeric(18,2)
                    USING "Amount"::numeric(18,2);
                END IF;

                -- CreatedAt: timestamp with time zone.
                SELECT data_type
                INTO current_type
                FROM information_schema.columns
                WHERE table_schema = 'public'
                  AND table_name = 'Payments'
                  AND column_name = 'CreatedAt';

                IF current_type = 'timestamp without time zone' THEN
                    ALTER TABLE "Payments"
                    ALTER COLUMN "CreatedAt" TYPE timestamp with time zone
                    USING "CreatedAt" AT TIME ZONE 'UTC';
                END IF;

                -- PaidAt: timestamp with time zone.
                SELECT data_type
                INTO current_type
                FROM information_schema.columns
                WHERE table_schema = 'public'
                  AND table_name = 'Payments'
                  AND column_name = 'PaidAt';

                IF current_type = 'timestamp without time zone' THEN
                    ALTER TABLE "Payments"
                    ALTER COLUMN "PaidAt" TYPE timestamp with time zone
                    USING "PaidAt" AT TIME ZONE 'UTC';
                END IF;

                -- PaymentMethod: text.
                SELECT data_type
                INTO current_type
                FROM information_schema.columns
                WHERE table_schema = 'public'
                  AND table_name = 'Payments'
                  AND column_name = 'PaymentMethod';

                IF current_type IS NOT NULL AND current_type <> 'text' THEN
                    ALTER TABLE "Payments"
                    ALTER COLUMN "PaymentMethod" TYPE text
                    USING "PaymentMethod"::text;
                END IF;

                -- PayeeName: text.
                SELECT data_type
                INTO current_type
                FROM information_schema.columns
                WHERE table_schema = 'public'
                  AND table_name = 'Payments'
                  AND column_name = 'PayeeName';

                IF current_type IS NOT NULL AND current_type <> 'text' THEN
                    ALTER TABLE "Payments"
                    ALTER COLUMN "PayeeName" TYPE text
                    USING "PayeeName"::text;
                END IF;

                -- ConfirmedByStaffId: text.
                SELECT data_type
                INTO current_type
                FROM information_schema.columns
                WHERE table_schema = 'public'
                  AND table_name = 'Payments'
                  AND column_name = 'ConfirmedByStaffId';

                IF current_type IS NOT NULL AND current_type <> 'text' THEN
                    ALTER TABLE "Payments"
                    ALTER COLUMN "ConfirmedByStaffId" TYPE text
                    USING "ConfirmedByStaffId"::text;
                END IF;

                -- Status: integer-backed PaymentStatus enum.
                SELECT data_type
                INTO current_type
                FROM information_schema.columns
                WHERE table_schema = 'public'
                  AND table_name = 'Payments'
                  AND column_name = 'Status';

                IF current_type IN ('character varying', 'character', 'text') THEN

                    SELECT COUNT(*)
                    INTO invalid_count
                    FROM "Payments"
                    WHERE "Status" IS NULL
                       OR LOWER(TRIM("Status"::text))
                          NOT IN (
                              'pending',
                              'confirmed',
                              'rejected',
                              '0',
                              '1',
                              '2'
                          );

                    IF invalid_count > 0 THEN
                        RAISE EXCEPTION
                            'Cannot convert Payments.Status: % invalid status value(s) found. Correct these values before retrying.',
                            invalid_count;
                    END IF;

                    ALTER TABLE "Payments"
                    ALTER COLUMN "Status" TYPE integer
                    USING (
                        CASE LOWER(TRIM("Status"::text))
                            WHEN 'pending' THEN 0
                            WHEN 'confirmed' THEN 1
                            WHEN 'rejected' THEN 2
                            WHEN '0' THEN 0
                            WHEN '1' THEN 1
                            WHEN '2' THEN 2
                        END
                    );
                END IF;
            END $$;
            """);

        // Drop the existing Bookings -> Rooms FK before recreating it.
        migrationBuilder.Sql(
            """
            ALTER TABLE "Bookings"
            DROP CONSTRAINT IF EXISTS "FK_Bookings_Rooms_RoomId";
            """);

        // Recreate the FK with Restrict behaviour.
        migrationBuilder.Sql(
            """
            DO $$
            BEGIN
                IF NOT EXISTS (
                    SELECT 1
                    FROM pg_constraint
                    WHERE conrelid = '"Bookings"'::regclass
                      AND conname = 'FK_Bookings_Rooms_RoomId'
                ) THEN
                    ALTER TABLE "Bookings"
                    ADD CONSTRAINT "FK_Bookings_Rooms_RoomId"
                    FOREIGN KEY ("RoomId")
                    REFERENCES "Rooms" ("Id")
                    ON DELETE RESTRICT;
                END IF;
            END $$;
            """);

        // Create ServiceOrders.GuestId index only when missing.
        migrationBuilder.Sql(
            """
            CREATE INDEX IF NOT EXISTS "IX_ServiceOrders_GuestId"
            ON "ServiceOrders" ("GuestId");
            """);

        // Add ServiceOrders -> Guests FK only when an equivalent FK is absent.
        migrationBuilder.Sql(
            """
            DO $$
            BEGIN
                IF NOT EXISTS (
                    SELECT 1
                    FROM pg_constraint c
                    JOIN pg_attribute a
                      ON a.attrelid = c.conrelid
                     AND a.attnum = ANY(c.conkey)
                    WHERE c.contype = 'f'
                      AND c.conrelid = '"ServiceOrders"'::regclass
                      AND c.confrelid = '"Guests"'::regclass
                      AND a.attname = 'GuestId'
                ) THEN
                    ALTER TABLE "ServiceOrders"
                    ADD CONSTRAINT "FK_ServiceOrders_Guests_GuestId"
                    FOREIGN KEY ("GuestId")
                    REFERENCES "Guests" ("Id")
                    ON DELETE RESTRICT;
                END IF;
            END $$;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Remove the ServiceOrders -> Guests FK if present.
        migrationBuilder.Sql(
            """
            ALTER TABLE "ServiceOrders"
            DROP CONSTRAINT IF EXISTS "FK_ServiceOrders_Guests_GuestId";
            """);

        migrationBuilder.Sql(
            """
            DROP INDEX IF EXISTS "IX_ServiceOrders_GuestId";
            """);

        // Restore the previous Bookings -> Rooms cascade behaviour.
        migrationBuilder.Sql(
            """
            ALTER TABLE "Bookings"
            DROP CONSTRAINT IF EXISTS "FK_Bookings_Rooms_RoomId";

            ALTER TABLE "Bookings"
            ADD CONSTRAINT "FK_Bookings_Rooms_RoomId"
            FOREIGN KEY ("RoomId")
            REFERENCES "Rooms" ("Id")
            ON DELETE CASCADE;
            """);

        // Remove IsActive. This removes values written to this column
        // after the migration was applied.
        migrationBuilder.Sql(
            """
            ALTER TABLE "AspNetUsers"
            DROP COLUMN IF EXISTS "IsActive";
            """);

        // Restore OrderItems.RoomNumber to varchar.
        migrationBuilder.Sql(
            """
            DO $$
            DECLARE
                current_type text;
            BEGIN
                SELECT data_type
                INTO current_type
                FROM information_schema.columns
                WHERE table_schema = 'public'
                  AND table_name = 'OrderItems'
                  AND column_name = 'RoomNumber';

                IF current_type = 'text' THEN
                    ALTER TABLE "OrderItems"
                    ALTER COLUMN "RoomNumber" TYPE character varying
                    USING "RoomNumber"::character varying;
                END IF;
            END $$;
            """);

        // Restore the previous Payments column types.
        migrationBuilder.Sql(
            """
            DO $$
            DECLARE
                current_type text;
            BEGIN
                -- Status: integer enum back to readable labels.
                SELECT data_type
                INTO current_type
                FROM information_schema.columns
                WHERE table_schema = 'public'
                  AND table_name = 'Payments'
                  AND column_name = 'Status';

                IF current_type = 'integer' THEN
                    ALTER TABLE "Payments"
                    ALTER COLUMN "Status" TYPE character varying(50)
                    USING (
                        CASE "Status"
                            WHEN 0 THEN 'Pending'
                            WHEN 1 THEN 'Confirmed'
                            WHEN 2 THEN 'Rejected'
                            ELSE "Status"::text
                        END
                    );
                END IF;

                -- ConfirmedByStaffId: varchar(255).
                SELECT data_type
                INTO current_type
                FROM information_schema.columns
                WHERE table_schema = 'public'
                  AND table_name = 'Payments'
                  AND column_name = 'ConfirmedByStaffId';

                IF current_type = 'text' THEN
                    ALTER TABLE "Payments"
                    ALTER COLUMN "ConfirmedByStaffId"
                    TYPE character varying(255)
                    USING "ConfirmedByStaffId"::character varying(255);
                END IF;

                -- PayeeName: varchar(255).
                SELECT data_type
                INTO current_type
                FROM information_schema.columns
                WHERE table_schema = 'public'
                  AND table_name = 'Payments'
                  AND column_name = 'PayeeName';

                IF current_type = 'text' THEN
                    ALTER TABLE "Payments"
                    ALTER COLUMN "PayeeName"
                    TYPE character varying(255)
                    USING "PayeeName"::character varying(255);
                END IF;

                -- PaymentMethod: varchar(255).
                SELECT data_type
                INTO current_type
                FROM information_schema.columns
                WHERE table_schema = 'public'
                  AND table_name = 'Payments'
                  AND column_name = 'PaymentMethod';

                IF current_type = 'text' THEN
                    ALTER TABLE "Payments"
                    ALTER COLUMN "PaymentMethod"
                    TYPE character varying(255)
                    USING "PaymentMethod"::character varying(255);
                END IF;

                -- PaidAt: timestamp without time zone.
                SELECT data_type
                INTO current_type
                FROM information_schema.columns
                WHERE table_schema = 'public'
                  AND table_name = 'Payments'
                  AND column_name = 'PaidAt';

                IF current_type = 'timestamp with time zone' THEN
                    ALTER TABLE "Payments"
                    ALTER COLUMN "PaidAt" TYPE timestamp without time zone
                    USING "PaidAt" AT TIME ZONE 'UTC';
                END IF;

                -- CreatedAt: timestamp without time zone.
                SELECT data_type
                INTO current_type
                FROM information_schema.columns
                WHERE table_schema = 'public'
                  AND table_name = 'Payments'
                  AND column_name = 'CreatedAt';

                IF current_type = 'timestamp with time zone' THEN
                    ALTER TABLE "Payments"
                    ALTER COLUMN "CreatedAt" TYPE timestamp without time zone
                    USING "CreatedAt" AT TIME ZONE 'UTC';
                END IF;

                -- Amount: unconstrained numeric.
                ALTER TABLE "Payments"
                ALTER COLUMN "Amount" TYPE numeric
                USING "Amount"::numeric;
            END $$;
            """);
    }
}