CREATE TABLE "Currencies" (
    "Id" INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY, 
    "CbrId" VARCHAR(10) NOT NULL,
    "CharCode" VARCHAR(3) NOT NULL,
    "Name" VARCHAR(100) NOT NULL,
    "Nominal" INT NOT NULL
);

CREATE UNIQUE INDEX "IX_Currencies_CbrId" 
ON "Currencies" ("CbrId");

CREATE TABLE "ExchangeRates" (
    "Id" BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY, 
    "CurrencyId" INT NOT NULL,
    "Date" DATE NOT NULL,
    "Value" NUMERIC(18, 4) NOT NULL, 
    CONSTRAINT "FK_ExchangeRates_Currencies_CurrencyId" 
        FOREIGN KEY ("CurrencyId") 
        REFERENCES "Currencies" ("Id") 
        ON DELETE CASCADE 
);

CREATE UNIQUE INDEX "IX_ExchangeRates_CurrencyId_Date" 
ON "ExchangeRates" ("CurrencyId", "Date");