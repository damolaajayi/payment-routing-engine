using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaymentRoutingEngine.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "outbox_messages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    aggregate_id = table.Column<Guid>(type: "uuid", nullable: false),
                    payload = table.Column<string>(type: "jsonb", nullable: false),
                    occurred_at_utc = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    processed_at_utc = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    error = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    retry_count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_outbox_messages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "payment_transactions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    reference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    idempotency_key = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    client_reference = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    customer_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    amount_minor = table.Column<long>(type: "bigint", nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    status = table.Column<short>(type: "smallint", nullable: false),
                    selected_provider = table.Column<short>(type: "smallint", nullable: true),
                    routing_decision_type = table.Column<short>(type: "smallint", nullable: true),
                    failure_category = table.Column<short>(type: "smallint", nullable: true),
                    failure_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    processed_at_utc = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    completed_at_utc = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment_transactions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "provider_routing_rules",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider = table.Column<short>(type: "smallint", nullable: false),
                    priority = table.Column<int>(type: "integer", nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    min_amount_minor = table.Column<long>(type: "bigint", nullable: true),
                    max_amount_minor = table.Column<long>(type: "bigint", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_provider_routing_rules", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "webhook_events",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider = table.Column<short>(type: "smallint", nullable: false),
                    external_event_id = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    signature = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    event_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    payload = table.Column<string>(type: "jsonb", nullable: false),
                    processing_status = table.Column<short>(type: "smallint", nullable: false),
                    received_at_utc = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    processed_at_utc = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    payment_transaction_id = table.Column<Guid>(type: "uuid", nullable: true),
                    error_message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_webhook_events", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "payment_attempts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    payment_transaction_id = table.Column<Guid>(type: "uuid", nullable: false),
                    attempt_number = table.Column<int>(type: "integer", nullable: false),
                    provider = table.Column<short>(type: "smallint", nullable: false),
                    status = table.Column<short>(type: "smallint", nullable: false),
                    provider_reference = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    provider_status_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    failure_category = table.Column<short>(type: "smallint", nullable: true),
                    failure_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_retry = table.Column<bool>(type: "boolean", nullable: false),
                    is_failover = table.Column<bool>(type: "boolean", nullable: false),
                    request_payload = table.Column<string>(type: "jsonb", nullable: true),
                    response_payload = table.Column<string>(type: "jsonb", nullable: true),
                    started_at_utc = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    completed_at_utc = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    next_retry_at_utc = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment_attempts", x => x.id);
                    table.ForeignKey(
                        name: "FK_payment_attempts_payment_transactions_payment_transaction_id",
                        column: x => x.payment_transaction_id,
                        principalTable: "payment_transactions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "transaction_status_history",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    payment_transaction_id = table.Column<Guid>(type: "uuid", nullable: false),
                    previous_status = table.Column<short>(type: "smallint", nullable: true),
                    new_status = table.Column<short>(type: "smallint", nullable: false),
                    reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    source = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    changed_at_utc = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    metadata = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transaction_status_history", x => x.id);
                    table.ForeignKey(
                        name: "FK_transaction_status_history_payment_transactions_payment_tra~",
                        column: x => x.payment_transaction_id,
                        principalTable: "payment_transactions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_outbox_messages_occurred_at_utc",
                table: "outbox_messages",
                column: "occurred_at_utc");

            migrationBuilder.CreateIndex(
                name: "IX_outbox_messages_processed_at_utc",
                table: "outbox_messages",
                column: "processed_at_utc");

            migrationBuilder.CreateIndex(
                name: "IX_payment_attempts_next_retry_at_utc",
                table: "payment_attempts",
                column: "next_retry_at_utc");

            migrationBuilder.CreateIndex(
                name: "IX_payment_attempts_payment_transaction_id_attempt_number",
                table: "payment_attempts",
                columns: new[] { "payment_transaction_id", "attempt_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payment_attempts_provider",
                table: "payment_attempts",
                column: "provider");

            migrationBuilder.CreateIndex(
                name: "IX_payment_attempts_status",
                table: "payment_attempts",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_payment_transactions_created_at_utc",
                table: "payment_transactions",
                column: "created_at_utc");

            migrationBuilder.CreateIndex(
                name: "IX_payment_transactions_idempotency_key",
                table: "payment_transactions",
                column: "idempotency_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payment_transactions_reference",
                table: "payment_transactions",
                column: "reference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payment_transactions_status",
                table: "payment_transactions",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_provider_routing_rules_provider_priority",
                table: "provider_routing_rules",
                columns: new[] { "provider", "priority" });

            migrationBuilder.CreateIndex(
                name: "IX_transaction_status_history_changed_at_utc",
                table: "transaction_status_history",
                column: "changed_at_utc");

            migrationBuilder.CreateIndex(
                name: "IX_transaction_status_history_payment_transaction_id",
                table: "transaction_status_history",
                column: "payment_transaction_id");

            migrationBuilder.CreateIndex(
                name: "IX_webhook_events_external_event_id",
                table: "webhook_events",
                column: "external_event_id");

            migrationBuilder.CreateIndex(
                name: "IX_webhook_events_processing_status",
                table: "webhook_events",
                column: "processing_status");

            migrationBuilder.CreateIndex(
                name: "IX_webhook_events_provider",
                table: "webhook_events",
                column: "provider");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "outbox_messages");

            migrationBuilder.DropTable(
                name: "payment_attempts");

            migrationBuilder.DropTable(
                name: "provider_routing_rules");

            migrationBuilder.DropTable(
                name: "transaction_status_history");

            migrationBuilder.DropTable(
                name: "webhook_events");

            migrationBuilder.DropTable(
                name: "payment_transactions");
        }
    }
}
