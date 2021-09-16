CREATE OR REPLACE FUNCTION "masterdata"."user_connectionid_upd"("p_userid" int8, "p_connectionid" text)
  RETURNS "pg_catalog"."void" AS $BODY$
BEGIN
	UPDATE masterdata."user" 
SET connectionid = p_connectionid,
		lastsignin = now()
WHERE
	userid = p_userid;
END;
$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100