-- AbayPOS SQLite schema. Money is stored as INTEGER cents.
-- Applied on first run. MySQL 8.4 can reuse the same table names and constraints.

PRAGMA foreign_keys = ON;

CREATE TABLE IF NOT EXISTS Users (
    Id              INTEGER PRIMARY KEY AUTOINCREMENT,
    Username        TEXT    NOT NULL UNIQUE COLLATE NOCASE,
    FullName        TEXT    NOT NULL,
    PasswordHash    TEXT    NOT NULL,
    Salt            TEXT    NOT NULL,
    Role            TEXT    NOT NULL CHECK (Role IN ('Cashier', 'Manager')),
    FailedAttempts  INTEGER NOT NULL DEFAULT 0,
    IsLocked        INTEGER NOT NULL DEFAULT 0 CHECK (IsLocked IN (0, 1)),
    IsActive        INTEGER NOT NULL DEFAULT 1 CHECK (IsActive IN (0, 1)),
    CreatedAt       TEXT    NOT NULL
);

CREATE TABLE IF NOT EXISTS Suppliers (
    Id        INTEGER PRIMARY KEY AUTOINCREMENT,
    Name      TEXT    NOT NULL,
    Phone     TEXT,
    Email     TEXT,
    Address   TEXT,
    IsActive  INTEGER NOT NULL DEFAULT 1 CHECK (IsActive IN (0, 1))
);

CREATE TABLE IF NOT EXISTS Products (
    Id              INTEGER PRIMARY KEY AUTOINCREMENT,
    Barcode         TEXT    NOT NULL UNIQUE,
    Name            TEXT    NOT NULL,
    Category        TEXT    NOT NULL,
    PriceCents      INTEGER NOT NULL CHECK (PriceCents > 0),
    CostPriceCents  INTEGER NOT NULL CHECK (CostPriceCents >= 0),
    StockQty        INTEGER NOT NULL CHECK (StockQty >= 0),
    ReorderLevel    INTEGER NOT NULL DEFAULT 0 CHECK (ReorderLevel >= 0),
    SupplierId      INTEGER REFERENCES Suppliers (Id),
    IsActive        INTEGER NOT NULL DEFAULT 1 CHECK (IsActive IN (0, 1)),
    Emoji           TEXT,
    ImagePath       TEXT
);

CREATE INDEX IF NOT EXISTS IX_Products_Name ON Products (Name);
CREATE INDEX IF NOT EXISTS IX_Products_Category ON Products (Category);
CREATE INDEX IF NOT EXISTS IX_Products_SupplierId ON Products (SupplierId);

CREATE TABLE IF NOT EXISTS Sales (
    Id               INTEGER PRIMARY KEY AUTOINCREMENT,
    SaleNo           TEXT    NOT NULL UNIQUE,
    CashierId        INTEGER NOT NULL REFERENCES Users (Id),
    SaleDate         TEXT    NOT NULL,
    SubtotalCents    INTEGER NOT NULL CHECK (SubtotalCents >= 0),
    DiscountCents    INTEGER NOT NULL CHECK (DiscountCents >= 0),
    TotalCents       INTEGER NOT NULL CHECK (TotalCents >= 0),
    PaymentMethod    TEXT    NOT NULL CHECK (PaymentMethod IN ('Cash', 'MobileMoney', 'Card')),
    AmountPaidCents  INTEGER NOT NULL CHECK (AmountPaidCents >= 0),
    ChangeCents      INTEGER NOT NULL CHECK (ChangeCents >= 0),
    Status           TEXT    NOT NULL CHECK (Status IN ('Completed', 'Voided'))
);

CREATE INDEX IF NOT EXISTS IX_Sales_SaleDate ON Sales (SaleDate);
CREATE INDEX IF NOT EXISTS IX_Sales_CashierId ON Sales (CashierId);
CREATE INDEX IF NOT EXISTS IX_Sales_Status ON Sales (Status);

CREATE TABLE IF NOT EXISTS SaleItems (
    Id              INTEGER PRIMARY KEY AUTOINCREMENT,
    SaleId          INTEGER NOT NULL REFERENCES Sales (Id),
    ProductId       INTEGER NOT NULL REFERENCES Products (Id),
    ProductName     TEXT    NOT NULL,
    Qty             INTEGER NOT NULL CHECK (Qty > 0),
    UnitPriceCents  INTEGER NOT NULL CHECK (UnitPriceCents >= 0),
    LineTotalCents  INTEGER NOT NULL CHECK (LineTotalCents >= 0)
);

CREATE INDEX IF NOT EXISTS IX_SaleItems_SaleId ON SaleItems (SaleId);
CREATE INDEX IF NOT EXISTS IX_SaleItems_ProductId ON SaleItems (ProductId);

CREATE TABLE IF NOT EXISTS StockReceipts (
    Id          INTEGER PRIMARY KEY AUTOINCREMENT,
    SupplierId  INTEGER NOT NULL REFERENCES Suppliers (Id),
    ReceivedBy  INTEGER NOT NULL REFERENCES Users (Id),
    ReceivedAt  TEXT    NOT NULL,
    Note        TEXT
);

CREATE TABLE IF NOT EXISTS StockReceiptItems (
    Id             INTEGER PRIMARY KEY AUTOINCREMENT,
    ReceiptId      INTEGER NOT NULL REFERENCES StockReceipts (Id),
    ProductId      INTEGER NOT NULL REFERENCES Products (Id),
    Qty            INTEGER NOT NULL CHECK (Qty > 0),
    UnitCostCents  INTEGER NOT NULL CHECK (UnitCostCents >= 0)
);

CREATE INDEX IF NOT EXISTS IX_StockReceiptItems_ReceiptId ON StockReceiptItems (ReceiptId);

CREATE TABLE IF NOT EXISTS AuditLog (
    Id         INTEGER PRIMARY KEY AUTOINCREMENT,
    UserId     INTEGER REFERENCES Users (Id),
    Action     TEXT    NOT NULL,
    Details    TEXT,
    CreatedAt  TEXT    NOT NULL
);

CREATE INDEX IF NOT EXISTS IX_AuditLog_CreatedAt ON AuditLog (CreatedAt);
CREATE INDEX IF NOT EXISTS IX_AuditLog_UserId ON AuditLog (UserId);
