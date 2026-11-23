-- Catalog seed. Users are created in code with the app PBKDF2 hasher
-- so default passwords stay in one place (admin123 / cashier123).

INSERT INTO Suppliers (Name, Phone, Email, Address, IsActive) VALUES
    ('Abay Distributors', '+251 911 000 111', 'abay@distributors.et', 'Merkato, Addis Ababa', 1),
    ('Highland Wholesale', '+251 922 000 222', 'sales@highland.et', 'Adama Industrial Zone', 1);

-- Prices in cents (ETB). A few SKUs sit at or below ReorderLevel for low-stock tests.
INSERT INTO Products (Barcode, Name, Category, PriceCents, CostPriceCents, StockQty, ReorderLevel, SupplierId, IsActive, Emoji) VALUES
    ('1001', 'Coca-Cola 500ml', 'Beverages',     4500,   2800,  80, 12, 1, 1, '🥤'),
    ('1002', 'Bread',           'Bakery',        2000,   1200,   8, 15, 2, 1, '🍞'),
    ('1003', 'Milk 1L',         'Dairy',         8500,   6200,   6, 10, 2, 1, '🥛'),
    ('1004', 'Rice 5kg',        'Staples',      65000,  52000,  24,  8, 1, 1, '🍚'),
    ('1005', 'Sugar 1kg',       'Staples',      12000,   9000,  40, 10, 1, 1, '🍬'),
    ('1006', 'Soap',            'Personal Care', 4000,   2200,  55, 12, 1, 1, '🧼'),
    ('1007', 'Water 500ml',     'Beverages',     1500,    700, 120, 20, 1, 1, '💧'),
    ('1008', 'Eggs 12pc',       'Dairy',        18000,  14000,  18,  8, 2, 1, '🥚'),
    ('1009', 'Cooking Oil 1L',  'Staples',      28000,  22000,  16,  6, 1, 1, '🛢️'),
    ('1010', 'Pasta 500g',      'Staples',       5500,   3800,  35, 10, 1, 1, '🍝'),
    ('1011', 'Black Tea 100g',  'Beverages',     7500,   5000,  22,  8, 2, 1, '🍵'),
    ('1012', 'Coffee 200g',     'Beverages',    22000,  16000,  14,  5, 2, 1, '☕'),
    ('1013', 'Biscuits',        'Snacks',        3500,   2000,  48, 12, 1, 1, '🍪'),
    ('1014', 'Toothpaste',      'Personal Care', 6500,   4000,  30,  8, 1, 1, '🪥'),
    ('1015', 'Shampoo 400ml',   'Personal Care', 9500,   6200,  12,  6, 1, 1, '🧴'),
    ('1016', 'Salt 1kg',        'Staples',       2500,   1400,   4, 10, 2, 1, '🧂'),
    ('1017', 'Tomato Sauce',    'Staples',       4500,   2800,  20,  8, 1, 1, '🍅'),
    ('1018', 'Instant Noodles', 'Snacks',        1800,   1000,  70, 20, 1, 1, '🍜');
