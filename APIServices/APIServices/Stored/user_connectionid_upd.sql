CREATE OR REPLACE FUNCTION "masterdata"."user_connectionid_upd"("p_username" text, "p_connectionid" text)
  RETURNS "pg_catalog"."void" AS $BODY$
BEGIN
	UPDATE masterdata."user" 
SET connectionid = p_connectionid,
		lastsignin = now()
WHERE
	username = p_username;
END;
$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100