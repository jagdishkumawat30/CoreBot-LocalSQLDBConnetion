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
    public class InsertDialog : CancelAndHelpDialog
    {
        UserRepository userRepository;
        public InsertDialog(UserRepository _userRepository)
            : base(nameof(InsertDialog))
        {
            userRepository = _userRepository;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                EmployeeIdStepAsync,
                EmployeeNameStepAsync,
                ActStepAsync,
                FinalStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> EmployeeIdStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
            {
                Prompt = MessageFactory.Text("Please enter the Employee Id.")
            }, cancellationToken);
        }

        private async Task<DialogTurnResult> EmployeeNameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["EmployeeId"] = (string)stepContext.Result;
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
            {
                Prompt = MessageFactory.Text($"Please enter the Employee Name for id {(string)stepContext.Values["EmployeeId"]}")
            }, cancellationToken);
        }

        private async Task<DialogTurnResult> ActStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["EmployeeName"] = (string)stepContext.Result;

            Employee employee = new Employee();
            employee.EmpId = (string)stepContext.Values["EmployeeId"];
            employee.EmpName = (string)stepContext.Values["EmployeeName"];

            bool status = userRepository.InsertEmployee(employee);

            if (status)
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Employee Inserted"), cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Employee Not Inserted or Employee already exists"), cancellationToken);
            }

            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions
            {
                Prompt = MessageFactory.Text("Would you like to insert more Employee details?")
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
