CREATE OR REPLACE FUNCTION "masterdata"."user_getbyuserpwd"("p_username" text, "p_password" text)
  RETURNS "pg_catalog"."void" AS $BODY$
BEGIN
	SELECT * FROM masterdata."user"
	WHERE "user".username = p_user
			AND "user"."password" = p_password;
END;
$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100