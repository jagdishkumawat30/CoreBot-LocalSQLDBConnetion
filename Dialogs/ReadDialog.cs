using CoreBotDBConnetion.Models;
using CoreBotDBConnetion.Utility;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CoreBotDBConnetion.Dialogs
{
    public class ReadDialog : CancelAndHelpDialog
    {
        UserRepository userRepository;
        public ReadDialog(UserRepository _userRepository)
            : base(nameof(ReadDialog))
        {
            userRepository = _userRepository;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                IntroStepAsync,
                ActStepAsync,
                FinalStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
            {
                Prompt = MessageFactory.Text("Please enter the Employee Id.")
            }, cancellationToken);
        }

        private async Task<DialogTurnResult> ActStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["EmployeeId"] = (string)stepContext.Result;
            Employee employee = userRepository.FetchEmployeeDetails((string)stepContext.Values["EmployeeId"]);
            if (employee == null)
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Employee with id {(string)stepContext.Values["EmployeeId"]} not found."), cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"{employee.EmpId} - {employee.EmpName}"), cancellationToken);
            }

            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions
            {
                Prompt = MessageFactory.Text("Would you like to search for more Employee names?")
            }, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if ((bool)stepContext.Result)
            {
                return await stepContext.ReplaceDialogAsync(InitialDialogId, null, cancellationToken);
            }
            else
            {
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
        }
    }
}
