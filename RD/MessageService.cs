using System.Windows.Forms;

namespace RD
{
    public interface IMessageService
    {

        void ShowMessage(string message);
        DialogResult ShowQuestion(string question);
        void ShowExclamation(string exclamation);
        void ShowError(string error);
    }

    class MessageService: IMessageService
    {

        public void ShowMessage(string message)
        {
            MessageBox.Show(message, "Повідомлення", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public DialogResult ShowQuestion(string question)
        {
            DialogResult dialogResult = MessageBox.Show(question, "Підтвердження", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            return dialogResult;
        }

        public void ShowExclamation(string exclamation)
        {
            MessageBox.Show(exclamation, "Попередження", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        public void ShowError(string error)
        {
            MessageBox.Show(error, "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
