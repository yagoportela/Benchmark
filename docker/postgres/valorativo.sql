CREATE TABLE IF NOT EXISTS public."Arquivo"
(
    "NomeArquivo" character varying(256) COLLATE pg_catalog."default" NOT NULL,
    "DescricaoArquivo" character varying(250) COLLATE pg_catalog."default",
    "NomeInternoArquivo" character varying(256) COLLATE pg_catalog."default",
    CONSTRAINT "Arquivo_pkey" PRIMARY KEY ("NomeArquivo")
);

CREATE TABLE IF NOT EXISTS public."Atributo"
(
    "NomeAtributo" character varying(256) COLLATE pg_catalog."default" NOT NULL,
    "DescricaoAtributo" character varying(256) COLLATE pg_catalog."default",
    "NomeInternoAtributo" character varying(256) COLLATE pg_catalog."default",
    CONSTRAINT "Atributo_pkey" PRIMARY KEY ("NomeAtributo")
);

CREATE TABLE IF NOT EXISTS public."Familia"
(
    "NomeFamilia" character varying(256) COLLATE pg_catalog."default" NOT NULL,
    "DescricaoFamilia" character varying(256) COLLATE pg_catalog."default",
    "NomeInternoFamilia" character varying(256) COLLATE pg_catalog."default",
    CONSTRAINT "Familia_pkey" PRIMARY KEY ("NomeFamilia")
);

CREATE TABLE IF NOT EXISTS public."Serie"
(
    "NomeSerie" character varying(256) COLLATE pg_catalog."default" NOT NULL,
    "DescricaoSerie" character varying(256) COLLATE pg_catalog."default",
    "NomeInternoSerie" character varying(256) COLLATE pg_catalog."default",
    CONSTRAINT "Serie_pkey" PRIMARY KEY ("NomeSerie")
);

CREATE TABLE IF NOT EXISTS public."Valor"
(
    "NomeArquivo" character varying(256) COLLATE pg_catalog."default" NOT NULL,
    "NomeFamilia" character varying(256) COLLATE pg_catalog."default" NOT NULL,
    "NomeSerie" character varying(256) COLLATE pg_catalog."default" NOT NULL,
    "NomeAtributo" character varying(256) COLLATE pg_catalog."default" NOT NULL,
    "Valor" double precision NOT NULL,
    "DataInicioVigencia" date NOT NULL,
    "DataFimVigencia" date NOT NULL,
    "DataAtualizacao" date NOT NULL,
    CONSTRAINT "Valor_pkey" PRIMARY KEY ("NomeArquivo", "NomeFamilia", "NomeSerie", "NomeAtributo", "DataInicioVigencia", "DataFimVigencia")
);

-- Adiciona a chave estrangeira para a tabela "Arquivo"
ALTER TABLE IF EXISTS public."Valor"
    ADD CONSTRAINT "FK_Arquivo" FOREIGN KEY ("NomeArquivo")
    REFERENCES public."Arquivo" ("NomeArquivo") MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE NO ACTION
    NOT VALID;

-- Adiciona a chave estrangeira para a tabela "Atributo"
ALTER TABLE IF EXISTS public."Valor"
    ADD CONSTRAINT "FK_Atributo" FOREIGN KEY ("NomeAtributo")
    REFERENCES public."Atributo" ("NomeAtributo") MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE NO ACTION
    NOT VALID;

-- Adiciona a chave estrangeira para a tabela "Familia"
ALTER TABLE IF EXISTS public."Valor"
    ADD CONSTRAINT "FK_Familia" FOREIGN KEY ("NomeFamilia")
    REFERENCES public."Familia" ("NomeFamilia") MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE NO ACTION
    NOT VALID;

-- Adiciona a chave estrangeira para a tabela "Serie"
ALTER TABLE IF EXISTS public."Valor"
    ADD CONSTRAINT "FK_Serie" FOREIGN KEY ("NomeSerie")
    REFERENCES public."Serie" ("NomeSerie") MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE NO ACTION
    NOT VALID;

END;