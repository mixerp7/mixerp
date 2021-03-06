﻿/********************************************************************************
Copyright (C) Binod Nepal, Mix Open Foundation (http://mixof.org).

This file is part of MixERP.

MixERP is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

MixERP is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with MixERP.  If not, see <http://www.gnu.org/licenses/>.
***********************************************************************************/

using MixERP.Net.Common.Helpers;
using MixERP.Net.Common.Models.Core;
using MixERP.Net.Common.Models.Transactions;
using MixERP.Net.Core.Modules.Finance.Data.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web.Script.Serialization;
using System.Web.Services;
using System.Web.Util;
using CollectionHelper = MixERP.Net.WebControls.StockTransactionFactory.Helpers.CollectionHelper;

namespace MixERP.Net.Core.Modules.Finance.Services
{
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    [System.Web.Script.Services.ScriptService]
    public class JournalVoucher : WebService
    {
        [WebMethod(EnableSession = true)]
        public decimal GetExchangeRate(string currencyCode)
        {
            int officeId = SessionHelper.GetOfficeId();
            decimal exchangeRate = Transaction.GetExchangeRate(officeId, currencyCode);

            return exchangeRate;
        }

        [WebMethod(EnableSession = true)]
        public long Save(DateTime valueDate, string referenceNumber, string data, int costCenterId, string attachmentsJSON)
        {
            Collection<JournalDetailsModel> details = CollectionHelper.GetJournalDetailCollection(data);

            JavaScriptSerializer js = new JavaScriptSerializer();
            Collection<PostgresqlAttachmentModel> attachments = js.Deserialize<Collection<PostgresqlAttachmentModel>>(attachmentsJSON);

            foreach (JournalDetailsModel model in details)
            {
                if (model.Debit > 0 && model.Credit > 0)
                {
                    throw new InvalidOperationException("Invalid data");
                }

                if (model.Debit == 0 && model.Credit == 0)
                {
                    throw new InvalidOperationException("Invalid data");
                }

                if (model.Credit < 0 || model.Debit < 0)
                {
                    throw new InvalidOperationException("Invalid data");
                }

                if (!AccountHelper.AccountNumberExists(model.AccountNumber))
                {
                    throw new InvalidOperationException("Invalid account " + model.AccountNumber);
                }

                if (model.Credit > 0)
                {
                    if (AccountHelper.IsCashAccount(model.AccountNumber))
                    {
                        if (!CashRepositories.CashRepositoryCodeExists(model.CashRepositoryCode))
                        {
                            throw new InvalidOperationException("Invalid cash repository " + model.CashRepositoryCode);
                        }

                        if (CashRepositories.GetBalance(model.CashRepositoryCode, model.CurrencyCode) < model.Credit)
                        {
                            throw new InvalidOperationException("Insufficient balance in cash repository.");
                        }
                    }
                }
            }

            decimal drTotal = (from detail in details select detail.LocalCurrencyDebit).Sum();
            decimal crTotal = (from detail in details select detail.LocalCurrencyCredit).Sum();

            if (drTotal != crTotal)
            {
                throw new InvalidOperationException("Referencing sides are not equal.");
            }

            return Transaction.Add(valueDate, referenceNumber, costCenterId, details, attachments);
        }

        [WebMethod(EnableSession = true)]
        public void Approve(long tranId, string reason)
        {
            int officeId = SessionHelper.GetOfficeId();
            int userId = SessionHelper.GetUserId();
            long loginId = SessionHelper.GetLogOnId();
            const int verificationStatusId = 2;

            Transaction.Verify(tranId, officeId, userId, loginId, verificationStatusId, reason);
        }

        [WebMethod(EnableSession = true)]
        public void Reject(long tranId, string reason)
        {
            int officeId = SessionHelper.GetOfficeId();
            int userId = SessionHelper.GetUserId();
            long loginId = SessionHelper.GetLogOnId();
            const int verificationStatusId = -3;

            Transaction.Verify(tranId, officeId, userId, loginId, verificationStatusId, reason);
        }
    }
}