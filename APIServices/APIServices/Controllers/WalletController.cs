using APIServices.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace APIServices.Controllers
{
    [ApiController]
    public class WalletController : BaseController
    {

        [HttpPost]
        public object GetPublicKey(string privateKey)
        {
            try
            {
                Crypto crypto = new Crypto();
                return new { publicKey = crypto.GetPublicKeyFromPrivateKey(privateKey) };
            }
            catch (Exception ex)
            {
                return ex.ToString();
                throw;
            }
        }

        [HttpPost]
        public object GetSignature(string privateKey, string message)
        {
            try
            {
                Crypto crypto = new Crypto();

                var publicKey = crypto.GetPublicKeyFromPrivateKeyEx(privateKey);

                var signature = crypto.Signature(privateKey, message);

                var isvalid = crypto.VerifySignature(message, publicKey, signature);

                return new { privateKey = privateKey, publicKey = publicKey, signature = signature, isvalid = isvalid };
            }
            catch (Exception ex)
            {
                return ex.ToString();
                throw;
            }
        }

        [HttpPost]
        public object VerifySignature(string message, string signature, string publicKey)
        {
            try
            {
                Crypto crypto = new Crypto();

                var isvalid = crypto.VerifySignature(message, publicKey, signature);

                return new { message = message, publicKey = publicKey, signature = signature, isvalid = isvalid };
            }
            catch (Exception ex)
            {
                return ex.ToString();
                throw;
            }
        }

        [HttpPost]
        public object NewTransaction(string toPublicKey, double amount)
        {
            try
            {
                Crypto cryto = new Crypto();
                var privateKey = "123456789";
                var publicKey = cryto.GetPublicKeyFromPrivateKeyEx(privateKey);
                var transaction = new Transaction();
                transaction.FromPublicKey = publicKey;
                transaction.ToPublicKey = toPublicKey == null ? "QrSNX7KxzGnQqauPiXKxP58nhukU252RKAmSqg17L8h7BpU984g4mxHck6cLzhArADz2p1xo3BwAsbiaLhQaziyu" : toPublicKey;
                transaction.Amount = amount;
                transaction.Signature = cryto.Signature(privateKey, transaction.ToString());

                bool isValidTransaction = cryto.VerifySignature(transaction.ToString(), publicKey, transaction.Signature);

                return new { transaction, isValidTransaction = isValidTransaction };
            }
            catch (Exception ex)
            {
                return ex.ToString();
                throw;
            }
        }
    }
}
