using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace GNIAPI.GNI
{
    class GNI
    {
        private string Token { get; set; }
        private string SESSIONID { get; set; }
        private string UserID { get; set; }
        public bool LoadComplet { get; set; }

        /// <summary>
        /// Iniciar conexão com o GNI
        /// </summary>
        /// <param name="TOKEN">Token da conta</param>
        public GNI (string TOKEN)
        {
            this.Token = TOKEN;
            this.LoadComplet = false;
            var dir = Directory.GetCurrentDirectory();
            if (File.Exists(@$"{dir}\Config\sessionid.txt"))
            {
                string[] sessions = File.ReadAllLines(@$"{dir}\Config\sessionid.txt");
                foreach (string L in sessions)
                {
                    var re = L.Split(';');
                    if (re.Length == 3)
                    {
                        if (re[0] == TOKEN)
                        {
                            this.SESSIONID = re[1];
                            this.UserID = re[2];
                            this.LoadComplet = true;
                            return;
                        }
                    }
                }
                if (this.SESSIONID != null)
                {
                    this.LoadComplet = true;
                    return;
                }
                else
                {
                    //Realizar login
                    var ret = GniController.LoginGNI(TOKEN);
                    if (ret.Status == "success")
                    {
                        SESSIONID = ret.SESSIONID;
                        UserID = ret.UserID;
                        string text = $"{TOKEN};{SESSIONID};{UserID}";
                        File.WriteAllText(@$"{dir}\Config\sessionid.txt", text);
                        this.LoadComplet = true;
                        return;
                    }
                    else
                    {
                        this.LoadComplet = false;
                        return;
                    }
                }
            }
            else
            {
                //Realizar novo login
                var ret = GniController.LoginGNI(TOKEN);
                if (ret.Status == "success")
                {
                    SESSIONID = ret.SESSIONID;
                    UserID = ret.UserID;
                    string text = $"{TOKEN};{SESSIONID};{UserID}";
                    File.WriteAllText(@$"{dir}\Config\sessionid.txt", text);
                    this.LoadComplet = true;
                    return;
                }
                else
                {
                    this.LoadComplet = false;
                    return;
                }
            }
        }

        /// <summary>
        /// Checar se a conta está cadastrada no GNI
        /// </summary>
        /// <param name="Username">Username</param>
        /// <returns></returns>
        public GNIRetoro CheckInsta (string Username)
        {
            GNIRetoro Ret = new GNIRetoro
            {
                Json = null,
                Response = "",
                Status = 0
            };
            var check = GniController.CheckAccount(this.Token, Username, this.SESSIONID);
            if (check.Status == "success")
            {
                Ret.Status = 1;
                Ret.Response = "Conta encontrada";
                Ret.Json = check;
                return Ret;
            } else
            {
                Ret.Status = -1;
                Ret.Response = "Não foi possivel localizar a conta";
                return Ret;
            }

        }

        /// <summary>
        /// Puxar uma tarefa para realizar
        /// </summary>
        /// <param name="ContaID">ID da conta</param>
        /// <returns></returns>
        public async Task<GNIRetoro> GetTask (string ContaID)
        {
            GNIRetoro Ret = new GNIRetoro
            {
                Json = null,
                Response = "",
                Status = 0
            };
            var task = GniController.GetTask(this.Token, ContaID, this.SESSIONID);
            if (task.Status != "ENCONTRADA")
            {
                var j = 0;
                bool taskCheck = false;
                while (j < 10 && taskCheck == false)
                {
                    task = GniController.GetTask(this.Token, ContaID, this.SESSIONID);
                    if (task.Status == "ENCONTRADA")
                    {
                        taskCheck = true;
                    }
                    await Task.Delay(2500);
                    j++;
                }
            }
            if (task.Status == "ENCONTRADA")
            {
                if (task.Tipo.ToLower() == "seguir")
                {
                    Ret.Status = 1;
                    Ret.Response = "Tipo: Seguir";
                }
                else
                {
                    Ret.Status = 2;
                    Ret.Response = "Tipo: Curtir";
                }
                Ret.Json = task;
                return Ret;
            } else
            {
                Ret.Status = -1;
                Ret.Response = "Não foi possivel localizar tarefa no momento";
                return Ret;
            }
        }
        
        /// <summary>
        /// Confirmar uma tarefa
        /// </summary>
        /// <param name="ContaID">ID da conta</param>
        /// <param name="TaskID">ID da tarefa</param>
        /// <returns></returns>
        public async Task<GNIRetoro> ConfirmTask (string ContaID, string TaskID)
        {
            GNIRetoro Ret = new GNIRetoro
            {
                Json = null,
                Response = "",
                Status = 0
            };
            var confirm = GniController.ConfirmTask(this.Token, ContaID, this.SESSIONID, TaskID, 1);
            if (confirm.Status == "interno")
            {
                int q = 0;
                while (q < 3 && confirm.Status == "interno")
                {
                    await Task.Delay(2500);
                    confirm = GniController.ConfirmTask(this.Token, ContaID, this.SESSIONID, TaskID, 1);
                    q++;
                }
            }
            if (confirm.Status == "success")
            {
                Ret.Status = 1;
                Ret.Response = "Tarefa confirmada";
                Ret.Json = confirm;
                return Ret;
            } else
            {
                Ret.Status = -1;
                Ret.Response = "Não foi posivel confirmar a tarefa";
                Ret.Json = confirm;
                return Ret;
            }
        }

        /// <summary>
        /// Pular uma tarefa
        /// </summary>
        /// <param name="ContaID">ID daconta</param>
        /// <param name="TaskID">ID da tarefa</param>
        /// <returns></returns>
        public async Task<GNIRetoro> JunpTask(string ContaID, string TaskID)
        {
            GNIRetoro Ret = new GNIRetoro
            {
                Json = null,
                Response = "",
                Status = 0
            };
            var confirm = GniController.ConfirmTask(this.Token, ContaID, this.SESSIONID, TaskID, 2);
            if (confirm.Status == "interno")
            {
                int q = 0;
                while (q < 3 && confirm.Status == "interno")
                {
                    await Task.Delay(2500);
                    confirm = GniController.ConfirmTask(this.Token, ContaID, this.SESSIONID, TaskID, 2);
                    q++;
                }
            }
            if (confirm.Status == "success")
            {
                Ret.Status = 1;
                Ret.Response = "Tarefa confirmada";
                Ret.Json = confirm;
                return Ret;
            }
            else
            {
                Ret.Status = -1;
                Ret.Response = "Não foi posivel confirmar a tarefa";
                Ret.Json = confirm;
                return Ret;
            }
        }

    }

    class GNIRetoro
    {
        public int Status { get; set; }
        public string Response { get; set; }
        public dynamic Json { get; set; }
    }

    class SenderLoginGNI
    {
        private string sha1 = "736e0a9928fc3407bf55c67ef77afcbe15303258";
        [JsonProperty("token")]
        public string Token { get; set; }
        [JsonProperty("sha1")]
        public string Sha1 { get { return sha1; } }
    }

    class SenderUsernameInsta
    {
        private string sha1 = "736e0a9928fc3407bf55c67ef77afcbe15303258";
        [JsonProperty("token")]
        public string Token { get; set; }
        [JsonProperty("sha1")]
        public string Sha1 { get { return sha1; } }
        [JsonProperty("SESSIONID")]
        public string SESSIONID { get; set; }
        [JsonProperty("nome_usuario")]
        public string Username { get; set; }
    }

    class SenderActionRequest
    {
        private string sha1 = "736e0a9928fc3407bf55c67ef77afcbe15303258";
        [JsonProperty("token")]
        public string Token { get; set; }
        [JsonProperty("sha1")]
        public string Sha1 { get { return sha1; } }
        [JsonProperty("SESSIONID")]
        public string SESSIONID { get; set; }
        [JsonProperty("id_conta")]
        public string ContaID { get; set; }
    }

    class SenderConfirmarTask
    {
        private string sha1 = "736e0a9928fc3407bf55c67ef77afcbe15303258";
        [JsonProperty("token")]
        public string Token { get; set; }
        [JsonProperty("sha1")]
        public string Sha1 { get { return sha1; } }
        [JsonProperty("SESSIONID")]
        public string SESSIONID { get; set; }
        [JsonProperty("id_conta")]
        public string ContaID { get; set; }
        [JsonProperty("id_pedido")]
        public string TaskID { get; set; }
        [JsonProperty("tipo")]
        public int Tipo { get; set; }
    }

    //Requisições

    //{"status":"success","id_conta":"XXXXXXXXXXXXXXX"}

    class ReturnID
    {
        [JsonProperty("status")]
        public string Status { get; set; }
        [JsonProperty("id_conta")]
        public string ContaID { get; set; }
    }

    //{"status":"fail","message":"EMPTY"}

    class ReturnStatus
    {
        [JsonProperty("status")]
        public string Status { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }
    }

    //{"status":"success","id_usuario":"XXXXXXXXXXXXXXX","email_usuario":"XXXXXXXXXXXXXXX","message":"SUCESSO_LOGIN","SESSIONID":"XXXXXXXXXXXXXXX"}

    class ReturnLogin
    {
        [JsonProperty("status")]
        public string Status { get; set; }
        [JsonProperty("id_usuario")]
        public string UserID { get; set; }
        [JsonProperty("email_usuario")]
        public string EmailGNI { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }
        [JsonProperty("SESSIONID")]
        public string SESSIONID { get; set; }
    }

    class Retorno
    {
        public int Status { get; set; }
        public string Response { get; set; }
    }

    //{"status":"ENCONTRADA","id_pedido":XXXXXXXXXXXXXXX,"url":"XXXXXXXXXXXXXXX","id_alvo":"XXXXXXXXXXXXXXX","tipo_acao":"TIPO_ACAO"}

    class ReturnTask
    {
        [JsonProperty("status")]
        public string Status { get; set; }
        [JsonProperty("id_pedido")]
        public string PedidoID { get; set; }
        [JsonProperty("url")]
        public string URL { get; set; }
        [JsonProperty("id_alvo")]
        public string AlvoID { get; set; }
        [JsonProperty("tipo_acao")]
        public string Tipo { get; set; }
    }

    class GniController
    {
        //Login - https://www.ganharnoinsta.com/api/login.php

        public static ReturnLogin LoginGNI(string Token)
        {
            var Receiver = new ReturnLogin
            {
                Status = "fail",
                Message = $"Não foi possivel conectar com a API.",
                EmailGNI = "",
                SESSIONID = "",
                UserID = ""
            };
            try
            {
                if (String.IsNullOrEmpty(Token))
                {
                    return Receiver;
                }
                SenderLoginGNI Sender = new SenderLoginGNI
                {
                    Token = Token
                };
                using (var cliente = new HttpClient())
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                    cliente.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    cliente.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Mozilla", "5.0"));
                    string adress = "https://www.ganharnoinsta.com/api/login.php";
                    var serializedSender = JsonConvert.SerializeObject(Sender);
                    var content = new StringContent(serializedSender, Encoding.UTF8, "application/json");
                    var request = cliente.PostAsync(adress, content).Result;
                    if (request.IsSuccessStatusCode)
                    {
                        var aux = request.Content.ReadAsStringAsync().Result;
                        if (aux.IndexOf("success") > -1)
                        {
                            Receiver = JsonConvert.DeserializeObject<ReturnLogin>(aux);
                            return Receiver;
                        }
                        else
                        {
                            var Retorno = JsonConvert.DeserializeObject<ReturnStatus>(aux);
                            Receiver.Message = Retorno.Message;
                            Receiver.Status = Retorno.Status;
                            return Receiver;
                        }
                    }
                    return Receiver;
                }
            }
            catch (Exception err)
            {
                Receiver.Status = "fail";
                Receiver.Message = $"Erro ao realizar a requisição: '{err.Message}'.";
                return null;
            }
        }

        //CheckAccount - https://www.ganharnoinsta.com/api/check_account.php

        public static ReturnID CheckAccount(string Token, string Username, string SESSIONID)
        {
            var Receiver = new ReturnID
            {
                Status = "fail",
                ContaID = "Não foi possivel conectar coma API."
            };
            try
            {
                if (String.IsNullOrEmpty(Username))
                {
                    return Receiver;
                }
                if (String.IsNullOrEmpty(SESSIONID))
                {
                    return Receiver;
                }
                if (String.IsNullOrEmpty(Token))
                {
                    return Receiver;
                }
                SenderUsernameInsta Sender = new SenderUsernameInsta
                {
                    Token = Token,
                    SESSIONID = SESSIONID,
                    Username = Username
                };
                using (var cliente = new HttpClient())
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                    cliente.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    cliente.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Mozilla", "5.0"));
                    string adress = "https://www.ganharnoinsta.com/api/check_account.php";
                    var serializedSender = JsonConvert.SerializeObject(Sender);
                    var content = new StringContent(serializedSender, Encoding.UTF8, "application/json");
                    var request = cliente.PostAsync(adress, content).Result;
                    if (request.IsSuccessStatusCode)
                    {
                        var aux = request.Content.ReadAsStringAsync().Result;
                        if (aux.IndexOf("success") > -1)
                        {
                            Receiver = JsonConvert.DeserializeObject<ReturnID>(aux);
                            return Receiver;
                        }
                        else
                        {
                            var Retorno = JsonConvert.DeserializeObject<ReturnStatus>(aux);
                            Receiver.ContaID = Retorno.Message;
                            Receiver.Status = Retorno.Status;
                            return Receiver;
                        }
                    }
                    return Receiver;
                }
            }
            catch (Exception err)
            {
                Receiver.Status = "fail";
                Receiver.ContaID = $"Erro ao realizar a requisição: '{err.Message}'.";
                return null;
            }
        }

        //Get Action - https://www.ganharnoinsta.com/api/get_action.php

        public static ReturnTask GetTask(string Token, string ContaID, string SESSIONID)
        {
            var Receiver = new ReturnTask
            {
                Status = "fail",
                Tipo = "Não foi possivel conectar coma API.",
                AlvoID = "",
                PedidoID = "",
                URL = ""
            };
            try
            {
                if (String.IsNullOrEmpty(ContaID))
                {
                    return Receiver;
                }
                if (String.IsNullOrEmpty(SESSIONID))
                {
                    return Receiver;
                }
                if (String.IsNullOrEmpty(Token))
                {
                    return Receiver;
                }
                SenderActionRequest Sender = new SenderActionRequest
                {
                    Token = Token,
                    SESSIONID = SESSIONID,
                    ContaID = ContaID
                };
                using (var cliente = new HttpClient())
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                    cliente.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    cliente.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Mozilla", "5.0"));
                    string adress = "https://www.ganharnoinsta.com/api/get_action.php";
                    var serializedSender = JsonConvert.SerializeObject(Sender);
                    var content = new StringContent(serializedSender, Encoding.UTF8, "application/json");
                    var request = cliente.PostAsync(adress, content).Result;
                    if (request.IsSuccessStatusCode)
                    {
                        var aux = request.Content.ReadAsStringAsync().Result;
                        if (aux.IndexOf("ENCONTRADA") > -1)
                        {
                            Receiver = JsonConvert.DeserializeObject<ReturnTask>(aux);
                            return Receiver;
                        }
                        else
                        {
                            if (aux.IndexOf("NAO_ENCONTRADA") > -1)
                            {
                                Receiver.Tipo = "Não foi encontrada tarefa para a conta atual";
                                Receiver.Status = "NAO_ENCONTRADA";
                                return Receiver;
                            }
                            else
                            {
                                var Retorno = JsonConvert.DeserializeObject<ReturnStatus>(aux);
                                Receiver.Tipo = Retorno.Message;
                                Receiver.Status = Retorno.Status;
                                return Receiver;
                            }
                        }
                    }
                    return Receiver;
                }
            }
            catch (Exception err)
            {
                Receiver.Status = "fail";
                Receiver.Tipo = $"Erro ao realizar a requisição: '{err.Message}'.";
                return null;
            }
        }

        //Confirm Action - https://www.ganharnoinsta.com/api/confirm_action.php

        public static ReturnStatus ConfirmTask(string Token, string ContaID, string SESSIONID, string TaskID, int Tipo)
        {
            var Receiver = new ReturnStatus
            {
                Status = "interno",
                Message = "Não foi possivel conectar coma API."
            };
            try
            {
                if (String.IsNullOrEmpty(ContaID))
                {
                    return Receiver;
                }
                if (String.IsNullOrEmpty(SESSIONID))
                {
                    return Receiver;
                }
                if (String.IsNullOrEmpty(Token))
                {
                    return Receiver;
                }
                SenderConfirmarTask Sender = new SenderConfirmarTask
                {
                    Token = Token,
                    SESSIONID = SESSIONID,
                    ContaID = ContaID,
                    TaskID = TaskID,
                    Tipo = Tipo
                };
                using (var cliente = new HttpClient())
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                    cliente.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    cliente.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Mozilla", "5.0"));
                    string adress = "https://www.ganharnoinsta.com/api/confirm_action.php";
                    var serializedSender = JsonConvert.SerializeObject(Sender);
                    var content = new StringContent(serializedSender, Encoding.UTF8, "application/json");
                    var request = cliente.PostAsync(adress, content).Result;
                    if (request.IsSuccessStatusCode)
                    {
                        var aux = request.Content.ReadAsStringAsync().Result;
                        if (aux.IndexOf("success") > -1)
                        {
                            Receiver = JsonConvert.DeserializeObject<ReturnStatus>(aux);
                            return Receiver;
                        }
                        else
                        {
                            Receiver = JsonConvert.DeserializeObject<ReturnStatus>(aux);
                            return Receiver;
                        }
                    }
                    return Receiver;
                }
            }
            catch (Exception err)
            {
                Receiver.Status = "interno";
                Receiver.Message = $"Erro ao realizar a requisição: '{err.Message}'.";
                return null;
            }
        }

    }

}
