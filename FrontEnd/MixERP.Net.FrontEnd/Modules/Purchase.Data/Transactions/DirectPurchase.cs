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

using System;
using System.Collections.ObjectModel;
using MixERP.Net.Common.Helpers;
using MixERP.Net.Common.Models.Core;
using MixERP.Net.Common.Models.Transactions;

namespace MixERP.Net.Core.Modules.Purchase.Data.Transactions
{
    public static class DirectPurchase
    {
        public static long Add(DateTime valueDate, int storeId, bool isCredit, string partyCode, Collection<StockMasterDetailModel> details, int costCenterId, string referenceNumber, string statementReference, Collection<PostgresqlAttachmentModel> attachments)
        {
            StockMasterModel stockMaster = new StockMasterModel();

            stockMaster.PartyCode = partyCode;
            stockMaster.StoreId = storeId;
            stockMaster.IsCredit = isCredit;

            if (!string.IsNullOrWhiteSpace(statementReference))
            {
                statementReference = statementReference.Replace("&nbsp;", " ").Trim();
            }

            long transactionMasterId = GlTransaction.Add(valueDate, "Purchase.Direct", SessionHelper.GetOfficeId(), SessionHelper.GetUserId(), SessionHelper.GetLogOnId(), costCenterId, referenceNumber, statementReference, stockMaster, details, new Collection<long>(), attachments);
            return transactionMasterId;
        }
    }
}