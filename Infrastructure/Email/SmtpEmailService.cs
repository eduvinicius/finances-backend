using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MyFinances.App.Abstractions;

namespace MyFinances.Infrastructure.Email
{
    public class SmtpEmailService(IConfiguration config, ILogger<SmtpEmailService> logger) : IEmailService
    {
        private readonly IConfiguration _config = config;
        private readonly ILogger<SmtpEmailService> _logger = logger;

        public async Task SendPasswordResetEmailAsync(string toEmail, string userName, string resetLink)
        {
            var host = _config["Email:SmtpHost"]!;
            var port = int.Parse(_config["Email:SmtpPort"]!);
            var user = _config["Email:SmtpUser"]!;
            var password = _config["Email:SmtpPassword"]!;
            var fromAddress = _config["Email:FromAddress"]!;
            var fromName = _config["Email:FromName"] ?? "MyFinances";

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, fromAddress));
            message.To.Add(new MailboxAddress(userName, toEmail));
            message.Subject = "Redefinição de senha — MyFinances";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $"""
                    <!DOCTYPE html>
                    <html lang="pt-BR">
                    <head><meta charset="UTF-8" /></head>
                    <body style="font-family: Arial, sans-serif; background: #f4f4f4; margin: 0; padding: 0;">
                      <table width="100%" cellpadding="0" cellspacing="0" style="background:#f4f4f4; padding: 40px 0;">
                        <tr>
                          <td align="center">
                            <table width="560" cellpadding="0" cellspacing="0" style="background:#ffffff; border-radius:8px; padding:40px;">
                              <tr>
                                <td>
                                  <h2 style="color:#1a1a1a; margin-bottom:8px;">Redefinição de senha</h2>
                                  <p style="color:#555; font-size:15px;">Olá, {userName}!</p>
                                  <p style="color:#555; font-size:15px;">
                                    Recebemos um pedido para redefinir a senha da sua conta no <strong>MyFinances</strong>.
                                    Clique no botão abaixo para criar uma nova senha. O link é válido por <strong>1 hora</strong>.
                                  </p>
                                  <div style="text-align:center; margin: 32px 0;">
                                    <a href="{resetLink}"
                                       style="background:#16a34a; color:#ffffff; padding:14px 28px; border-radius:6px;
                                              text-decoration:none; font-size:15px; font-weight:bold;">
                                      Redefinir senha
                                    </a>
                                  </div>
                                  <p style="color:#888; font-size:13px;">
                                    Se você não solicitou a redefinição de senha, ignore este e-mail — sua senha não será alterada.
                                  </p>
                                  <hr style="border:none; border-top:1px solid #eee; margin: 24px 0;" />
                                  <p style="color:#aaa; font-size:12px; text-align:center;">
                                    Se o botão não funcionar, copie e cole o link abaixo no seu navegador:<br />
                                    <a href="{resetLink}" style="color:#16a34a; word-break:break-all;">{resetLink}</a>
                                  </p>
                                </td>
                              </tr>
                            </table>
                          </td>
                        </tr>
                      </table>
                    </body>
                    </html>
                    """,
                TextBody = $"""
                    Redefinição de senha — MyFinances

                    Olá, {userName}!

                    Recebemos um pedido para redefinir a senha da sua conta.
                    Acesse o link abaixo para criar uma nova senha (válido por 1 hora):

                    {resetLink}

                    Se você não solicitou essa redefinição, ignore este e-mail.
                    """
            };

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(host, port, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(user, password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Password reset email sent to {Email}", toEmail);
        }
    }
}
