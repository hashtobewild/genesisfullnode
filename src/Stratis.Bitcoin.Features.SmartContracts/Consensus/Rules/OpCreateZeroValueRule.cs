﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NBitcoin;
using Stratis.Bitcoin.Features.Consensus;
using Stratis.Bitcoin.Features.Consensus.Rules;
using Stratis.SmartContracts;
using Block = NBitcoin.Block;

namespace Stratis.Bitcoin.Features.SmartContracts.Consensus.Rules
{
    /// <summary>
    /// Validates that no satoshis were supplied in the smart contract create transaction
    /// </summary>
    [ValidationRule(CanSkipValidation = false)]
    public class OpCreateZeroValueRule : ConsensusRule
    {
        public override Task RunAsync(RuleContext context)
        {
            Block block = context.BlockValidationContext.Block;

            IEnumerable<Transaction> smartContractTransactions =
                block.Transactions.GetSmartContractCreateTransactions().ToList();

            if (!smartContractTransactions.Any())
            {
                // No smart contract transactions, nothing to validate
                return Task.CompletedTask;
            }

            foreach (Transaction transaction in smartContractTransactions)
            {
                // This should never be null as every tx in smartContractTransactions contains a SmartContractOutput
                // So throw if null, because we really didn't expect that
                TxOut smartContractOutput = transaction.Outputs.First(txOut => txOut.ScriptPubKey.IsSmartContractExec);

                var carrier = SmartContractCarrier.Deserialize(transaction, smartContractOutput);

                if (carrier.TxOutValue != 0)
                {
                    this.Throw();
                }
            }

            return Task.CompletedTask;
        }

        private void Throw()
        {
            // TODO make nicer
            new ConsensusError("op-create-had-nonzero-value",
                "op create had nonzero value").Throw();
        }
    }
}