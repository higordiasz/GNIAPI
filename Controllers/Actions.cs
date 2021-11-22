using GNIAPI.Controllers.HelpInstaAPI;
using GNIAPI.Models.ActionModels;
using InstagramApiSharp.API;
using InstagramApiSharp.API.Builder;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Logger;
using System;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using InstagramApiSharp.Classes.Android.DeviceInfo;
using System.Linq;

namespace GNIAPI.Controllers.Actions
{
    public class PostMidia
    {
        public int Status { get; set; }
        public string Mensagem { get; set; }
        public string Pk { get; set; }
        public string Url { get; set; }
    }
    public class Actions
    {
        private readonly UserSessionData userSession;

        private readonly Bot botMain;

        public Actions(UserSessionData UserSession, Bot BotMain)
        {
            userSession = UserSession;
            botMain = BotMain;
        }

        private string DateString()
        {
            var data = DateTime.Today;
            var dia = data.Day.ToString();
            var mes = data.Month.ToString();
            var ano = data.Year.ToString();
            return $"{dia}-{mes}-{ano}";
        }

        private string HorarioString()
        {
            return DateTime.Now.ToString("HH:mm:ss");
        }

        private void LogMessage(string message)
        {
            var dir = Directory.GetCurrentDirectory();
            if (Directory.Exists($@"{dir}\logs"))
            {
                var data = DateString();
                if (File.Exists($@"{dir}\logs\{data}.txt"))
                {
                    string[] linhas = File.ReadAllLines($@"{dir}\logs\{data}.txt");
                    var list = linhas.ToList();
                    list.Add($"GNIAPI {HorarioString()} {message}");
                    File.WriteAllLines($@"{dir}\logs\{data}.txt", list);
                    return;
                }
                else
                {
                    string[] linhas = { $"GNIAPI {HorarioString()} {message}" };
                    File.WriteAllLines($@"{dir}\logs\{data}.txt", linhas);
                    return;
                }
            }
            else
            {
                Directory.CreateDirectory($@"{dir}\logs");
                var data = DateString();
                if (File.Exists($@"{dir}\logs\{data}.txt"))
                {
                    string[] linhas = File.ReadAllLines($@"{dir}\logs\{data}.txt");
                    var list = linhas.ToList();
                    list.Add($"GNIAPI {HorarioString()} {message}");
                    File.WriteAllLines($@"{dir}\logs\{data}.txt", list);
                    return;
                }
                else
                {
                    string[] linhas = { $"GNIAPI {HorarioString()} {message}" };
                    File.WriteAllLines($@"{dir}\logs\{data}.txt", linhas);
                    return;
                }
            }
        }

        public async Task<ActionModel> DoLogin(string device, HttpClientHandler httpClient = null)
        {
            ActionModel dataAction = new ActionModel
            {
                Type = "Login",
                Username = userSession.UserName,
                Status = 0
            };
            try
            {
                var _device = AndroidDeviceGenerator.GetRandomAndroidDevice();
                //var _device = AndroidDeviceGenerator.GetByName(device);
                ApiRequestMessage r = new ApiRequestMessage
                {
                    PhoneId = _device.PhoneGuid.ToString(),
                    Guid = _device.DeviceGuid,
                    DeviceId = ApiRequestMessage.GenerateDeviceIdFromGuid(_device.DeviceGuid)
                };
                IInstaApiBuilder instaApiBuild = InstaApiBuilder.CreateBuilder()
                    .SetUser(userSession)
                    .SetApiRequestMessage(r)
                    .UseLogger(new DebugLogger(LogLevel.Exceptions));
                if (httpClient != null)
                {
                    instaApiBuild = instaApiBuild.UseHttpClientHandler(httpClient);
                }
                HelpersInstaApi.InstaApi = instaApiBuild.Build();
                HelpersInstaApi.SetDevice(_device);
                var dir = Directory.GetCurrentDirectory();
                string stateFile = $@"{dir}/Insta/{userSession.UserName}.bin";
                try
                {
                    // load session file if exists
                    if (File.Exists(stateFile))
                    {
                        using (var fs = File.OpenRead(stateFile))
                        {
                            HelpersInstaApi.InstaApi.LoadStateDataFromString(new StreamReader(fs).ReadToEnd());
                        }
                    }
                } catch {  }
                if (!HelpersInstaApi.InstaApi.IsUserAuthenticated)
                {
                    var loginResult = await HelpersInstaApi.InstaApi.LoginAsync();
                    if (loginResult.Succeeded)
                    {
                        dataAction.Status = 1;
                        dataAction.Response = $"[+] Login | Status: Success | Username: {userSession.UserName}";
                    }
                    else
                    {
                        switch (loginResult.Value)
                        {
                            case InstaLoginResult.InvalidUser:
                                dataAction.Response = $"[+] Login | Status: Failed | Error: Username is invalid.";
                                dataAction.Status = 4;
                                break;
                            case InstaLoginResult.BadPassword:
                                dataAction.Response = $"[+] Login | Status: Failed | Error: Password is wrong.";
                                dataAction.Status = 4;
                                break;
                            case InstaLoginResult.Exception:
                                dataAction.Response = $"[+] Login | Status: Failed | Error: {loginResult.Info.Message}";
                                break;
                            case InstaLoginResult.LimitError:
                                dataAction.Response = $"[+] Login | Status: Failed | Error: Limit error (you should wait 10 minutes).";
                                dataAction.Status = 3;
                                break;
                            case InstaLoginResult.ChallengeRequired:
                                dataAction.Status = 2;
                                dataAction.Response = $"[+] Login | Status: Failed | Error: Challenge Required.";
                                /*if (HelpersInstaApi.Challenge)
                                {
                                    HandleChallenge(dataAction);
                                }*/
                                break;
                            case InstaLoginResult.TwoFactorRequired:
                                dataAction.Response = $"[+] Login | Status: Failed | Error:  Factor Required. Disabled it first!";
                                break;
                            case InstaLoginResult.InactiveUser:
                                dataAction.Response = $"[+] Login | Status: Failed | Error:  {loginResult.Info.Message}";
                                break;
                        }
                    }
                }
                else
                {
                    dataAction.Status = 1;
                    dataAction.Response = $"[+] Login | Status: Success | Username: {userSession.UserName} | Carregada a sessão anterior.";
                }
            }
            catch (Exception ex)
            {
                dataAction.Response = $"[+] Login | Status: Failed | Error:  {ex.Message}";
            }
            return dataAction;
        }

        private static async void HandleChallenge(ActionModel dataAction)
        {
            try
            {
                IResult<InstaChallengeRequireVerifyMethod> challenge = null;
                challenge = await HelpersInstaApi.InstaApi.GetChallengeRequireVerifyMethodAsync();
                if (challenge.Succeeded)
                {
                    if (challenge.Value.StepData != null)
                    {
                        if (!string.IsNullOrEmpty(challenge.Value.StepData.PhoneNumber))
                            dataAction.Numero = challenge.Value.StepData.PhoneNumber;
                        string numero = $"[ 1 ] Challange: {challenge.Value.StepData.PhoneNumber}";
                        if (!string.IsNullOrEmpty(challenge.Value.StepData.Email))
                            dataAction.Email = challenge.Value.StepData.Email;
                        string email = $"[ 2 ] Challange: {challenge.Value.StepData.Email}";
                    }
                    dataAction.ChallengeResponse = "sucess";
                }
                else
                {
                    dataAction.ChallengeResponse = $"Challange Error: {challenge.Info.Message}";
                    return;
                }
            }
            catch (Exception ex)
            {
                dataAction.ChallengeResponse = $"Challange Error: {ex.Message}";
                return;
            }
        }

        public async Task SendCode()
        {
            HelpersInstaApi.WriteFullLine("[+] Opção de Verificação [1 or 2]: ", ConsoleColor.Green);
            var options = Console.ReadLine();
            if (HelpersInstaApi.InstaApi == null)
                return;
            try
            {
                if (int.TryParse(options, out int option))
                {
                    if (option == 1)
                    {
                        var phoneNumber = await HelpersInstaApi.InstaApi.RequestVerifyCodeToSMSForChallengeRequireAsync();
                        if (phoneNumber.Succeeded)
                        {
                            HelpersInstaApi.WriteFullLine($"[+] Foi enviado o código para o numero: {phoneNumber.Value.StepData.ContactPoint}", ConsoleColor.Green);
                        }
                        else
                        {
                            HelpersInstaApi.WriteFullLine($"[+] Erro ao responder o challenge: {phoneNumber.Info.Message}", ConsoleColor.Yellow);
                            return;
                        }
                    }
                    else if (option == 2)
                    {
                        var email = await HelpersInstaApi.InstaApi.RequestVerifyCodeToEmailForChallengeRequireAsync();
                        if (email.Succeeded)
                        {
                            HelpersInstaApi.WriteFullLine($"[+] Código enviado para o email: {email.Value.StepData.ContactPoint}", ConsoleColor.Green);
                        }
                        else
                        {
                            HelpersInstaApi.WriteFullLine($"[+] Erro ao responder challenge: {email.Info.Message}", ConsoleColor.Yellow);
                            return;
                        }
                    }
                    else
                    {
                        HelpersInstaApi.WriteFullLine("[+] Opção invalida!", ConsoleColor.Yellow);
                        return;
                    }
                }
                else
                {
                    HelpersInstaApi.WriteFullLine("[+] Opção invalida, coloque apenas numeros!", ConsoleColor.Yellow);
                    return;
                }
            }
            catch (Exception ex)
            {

                HelpersInstaApi.WriteFullLine($"Challange Error: {ex.Message}", ConsoleColor.Yellow);
                return;
            }
        }

        public async Task<ActionModel> VerifyCode(string code)
        {
            ActionModel dataAction = new ActionModel
            {
                Type = "VerifyCode",
                Username = userSession.UserName,
                Status = 0
            };
            try
            {
                var regex = new Regex(@"^-*[0-9,\.]+$");
                if (!regex.IsMatch(code))
                {
                    dataAction.Response = $"[+] {dataAction.Type} | Status: Failed | Error: Verification code is numeric!";
                }
                if (code.Length != 6)
                {
                    dataAction.Response = $"[+] {dataAction.Type} | Status: Failed | Error: Verification code must be 6 digits!";
                }
                var verify = await HelpersInstaApi.InstaApi.VerifyCodeForChallengeRequireAsync(code);
                if (verify.Succeeded)
                {
                    dataAction.Status = 1;
                    dataAction.Response = $"[+] {dataAction.Type} | Status: Success | Username: {userSession.UserName} { Environment.NewLine}";
                }
                else
                {
                    if (verify.Value == InstaLoginResult.TwoFactorRequired)
                    {
                        dataAction.Response = $"[+] {dataAction.Type} | Status: Failed | Error: Two Factor Required. Disabled it first!";
                    }
                }
            }
            catch (Exception ex)
            {
                dataAction.Response = $"[+] {dataAction.Type} | Status: Failed | Error: {ex.Message}!";
            }

            return dataAction;
        }

        public async Task<ActionModel> DoFollow(long userPk)
        {
            ActionModel dataAction = new ActionModel
            {
                Type = "Follow",
                Username = null,
                Status = 0
            };
            try
            {
                var follow = await HelpersInstaApi.InstaApi.UserProcessor.FollowUserAsync(userPk);
                if (follow.Succeeded)
                {
                    dataAction.Status = 1;
                    dataAction.Response = $"[+] {dataAction.Type} | Status: Success";
                }
                else
                {
                    dataAction.Response = $"[+] {dataAction.Type} | Status: Failed | Error: {follow.Info.Message}";
                }

            }
            catch (Exception ex)
            {

                dataAction.Response = $"[+] {dataAction.Type} | Status: Failed | Error: {ex.Message}";
            }
            return dataAction;
        }

        public async Task<ActionModel> DoLike(string mediaPk)
        {
            ActionModel dataAction = new ActionModel
            {
                Type = "Like",
                Username = userSession.UserName,
                Status = 0
            };
            try
            {
                var doLike = await HelpersInstaApi.InstaApi.MediaProcessor.LikeMediaAsync(mediaPk);
                if (doLike.Succeeded)
                {
                    dataAction.Status = 1;
                    dataAction.Response = $"[+] {dataAction.Type} | Status: Success";
                }
                else
                {
                    dataAction.Response = $"[+] {dataAction.Type} | Status: Failed | Error: {doLike.Info.Message}";
                }

            }
            catch (Exception ex)
            {
                dataAction.Response = $"[+] {dataAction.Type} | Status: Failed | Error: {ex.Message}";
            }
            return dataAction;
        }

        public async Task<ActionModel> DoComment(string media, string comments)
        {
            ActionModel dataAction = new ActionModel
            {
                Type = "Comment",
                Username = userSession.UserName,
                Status = 0
            };

            try
            {
                var comment = await HelpersInstaApi.InstaApi.CommentProcessor.CommentMediaAsync(media, comments);
                if (comment.Succeeded)
                {
                    dataAction.Status = 1;
                    dataAction.Response = $"[+] {dataAction.Type} | Status: Success | Text: {comment.Value.Text}";
                }
                else
                {
                    dataAction.Response = $"[+] {dataAction.Type} | Status: Failed | Error: {comment.Info.Message}";
                }

            }
            catch (Exception ex)
            {
                dataAction.Response = $"[+] {dataAction.Type} | Status: Failed | Error: {ex.Message}";
            }

            return dataAction;
        }

        public async Task<PostMidia> DoPostPhoto(string caminho, string description)
        {
            PostMidia dataAction = new PostMidia
            {
                Status = 0,
                Mensagem = null,
                Pk = null,
                Url = null
            };

            var mediaImage = new InstaImageUpload
            {
                Height = 0,
                Width = 0,
                Uri = caminho
            };

            try
            {
                var post = await HelpersInstaApi.InstaApi.MediaProcessor.UploadPhotoAsync(mediaImage, description);
                if (post.Succeeded)
                {
                    dataAction.Status = 1;
                    dataAction.Mensagem = $"Sucesso ao postar a foto";
                    dataAction.Pk = "" + post.Value.Pk;
                    dataAction.Url = "https://instagram.com/" + post.Value.Code + "/";
                }
                else
                {
                    dataAction.Status = 2;
                    dataAction.Mensagem = $"Status: Failed | Error: {post.Info.Message}";
                }
            }
            catch (Exception ex)
            {
                dataAction.Status = 0;
                dataAction.Mensagem = $"Status: Failed | Error: {ex.Message}";
            }
            return dataAction;
        }

        public async Task<PostMidia> DoPostStory(string caminho, string description)
        {
            PostMidia dataAction = new PostMidia
            {
                Status = 0,
                Mensagem = null,
                Pk = null,
                Url = null
            };

            var Image = new InstaImage
            {
                Height = 0,
                Width = 0,
                Uri = caminho
            };

            try
            {
                var post = await HelpersInstaApi.InstaApi.StoryProcessor.UploadStoryPhotoAsync(Image, description);
                if (post.Succeeded)
                {
                    dataAction.Status = 1;
                    dataAction.Mensagem = $"Sucesso ao postar a foto";
                    dataAction.Pk = "" + post.Value.Media.Pk;
                    dataAction.Url = "https://instagram.com/" + post.Value.Media.Code + "/";
                }
                else
                {
                    dataAction.Status = 2;
                    dataAction.Mensagem = $"Status: Failed | Error: {post.Info.Message}";
                }
            }
            catch (Exception ex)
            {
                dataAction.Status = 0;
                dataAction.Mensagem = $"Status: Failed | Error: {ex.Message}";
            }
            return dataAction;
        }

        public async Task<string> GetMyProfilePicUrl()
        {
            try
            {
                var res = await HelpersInstaApi.InstaApi.GetCurrentUserAsync();
                if (res.Succeeded)
                {
                    if (res.Value != null)
                    {
                        if (res.Value.HdProfilePicture != null)
                        {
                            if (res.Value.HdProfilePicture.Uri != null)
                            {
                                string hd = res.Value.HdProfilePicture.Uri;
                                return hd;
                            }
                        }
                    }
                    return "https://imgur.com/b24Rzo7.jpg";
                } else
                {
                    return "erro";
                }
            } catch
            {
                return "erro";
            }
        }

        public async Task<PostMidia> DoPostVideo(string c_video, string c_thumbnail, string description)
        {
            PostMidia dataAction = new PostMidia
            {
                Status = 0,
                Mensagem = null,
                Pk = null,
                Url = null
            };

            var video = new InstaVideoUpload
            {
                Video = new InstaVideo(c_video, 0, 0),
                VideoThumbnail = new InstaImage(c_thumbnail, 0, 0)
            };


            try
            {
                var post = await HelpersInstaApi.InstaApi.MediaProcessor.UploadVideoAsync(video, description);
                if (post.Succeeded)
                {
                    dataAction.Status = 1;
                    dataAction.Mensagem = $"Sucesso ao postar a foto";
                    dataAction.Pk = "" + post.Value.Pk;
                    dataAction.Url = "https://instagram.com/" + post.Value.Pk + "/";
                }
                else
                {
                    dataAction.Status = 2;
                    dataAction.Mensagem = $"Status: Failed | Error: {post.Info.Message}";
                }
            }
            catch (Exception ex)
            {
                dataAction.Status = 0;
                dataAction.Mensagem = $"Status: Failed | Error: {ex.Message}";
            }
            return dataAction;
        }

        public async Task<ActionModel> DoHumanization()
        {
            ActionModel dataAction = new ActionModel
            {
                Type = "Humanization",
                Username = userSession.UserName,
                Status = 1
            };
            var r = new Random();
            var feed = await HelpersInstaApi.InstaApi.FeedProcessor.GetFollowingRecentActivityFeedAsync(InstagramApiSharp.PaginationParameters.Empty);
            if (feed.Succeeded)
            {
                await Task.Delay(3894);
                if (feed != null)
                {
                    if (feed.Value != null)
                    {
                        if (feed.Value.Items != null)
                        {
                            if (feed.Value.Items.Count > 0)
                            {
                                //await this.DoLike(feed.Value.Items[0].Medias[0].Id);
                            }
                        }
                    }
                }
                if (true)
                {
                    var feed2 = await HelpersInstaApi.InstaApi.FeedProcessor.GetFollowingRecentActivityFeedAsync(InstagramApiSharp.PaginationParameters.Empty);
                    await Task.Delay(2687);
                    if (feed2 != null)
                    {
                        if (feed2.Value != null)
                        {
                            if (feed2.Value.Items != null)
                            {
                                if (feed2.Value.Items.Count > 0)
                                {
                                    //await this.DoLike(feed2.Value.Items[0].Medias[0].Id);
                                }
                            }
                        }
                    }
                    if (true)
                    {
                        await Task.Delay(3487);
                        var story = await HelpersInstaApi.InstaApi.StoryProcessor.GetStoryFeedAsync();
                        if (story.Succeeded)
                        {
                            await Task.Delay(2687);
                            if (story.Value != null)
                            {
                                if (story.Value.Items != null)
                                {
                                    if (story.Value.Items.Count > 0)
                                    {
                                        await HelpersInstaApi.InstaApi.StoryProcessor.MarkStoryAsSeenAsync(story.Value.Items[0].Id, 3);
                                    }
                                }
                            }
                            if (true)
                            {
                                feed = await HelpersInstaApi.InstaApi.FeedProcessor.GetRecentActivityFeedAsync(InstagramApiSharp.PaginationParameters.Empty);
                                if (feed != null)
                                {
                                    if (feed.Value != null)
                                    {
                                        if (feed.Value.Items != null)
                                        {
                                            if (feed.Value.Items.Count > 0)
                                            {
                                                //await this.DoComment(feed.Value.Items[0].Medias[0].Id, "🔥🔥");
                                            }
                                        }
                                    }
                                }
                                await Task.Delay(2687);
                            }
                        }
                    }
                }
            }
            return dataAction;
        }

        public async Task<ActionModel> DoSeeStoryOne()
        {
            ActionModel dataAction = new ActionModel
            {
                Type = "SeeStory",
                Username = userSession.UserName,
                Status = 1,
                Response = ""
            };
            try
            {
                var story = await HelpersInstaApi.InstaApi.StoryProcessor.GetStoryFeedAsync();
                if (story.Succeeded)
                {
                    await Task.Delay(2687);
                    if (story.Value != null)
                    {
                        if (story.Value.Items != null)
                        {
                            if (story.Value.Items.Count > 0)
                            {
                                await HelpersInstaApi.InstaApi.StoryProcessor.MarkStoryAsSeenAsync(story.Value.Items[0].Id, 4);
                            }
                        }
                    }
                }
                return dataAction;
            } catch (Exception err)
            {
                dataAction.Response = $"Erro: {err.Message}";
                dataAction.Status = 0;
                LogMessage("Assistir Story erro - " + dataAction.Response);
                return dataAction;
            }
        }

        public async Task<ActionModel> DoSeeStoryUserPk(long pk)
        {
            ActionModel dataAction = new ActionModel
            {
                Type = "SeePerfilStory",
                Username = userSession.UserName,
                Status = 1,
                Response = ""
            };
            try
            {
                var stroy = await HelpersInstaApi.InstaApi.StoryProcessor.GetUserStoryAsync(pk);
                if (stroy.Succeeded)
                {
                    if (stroy.Value.Items != null)
                    {
                        if (stroy.Value.Items.Count > 0)
                        {
                            await HelpersInstaApi.InstaApi.StoryProcessor.MarkStoryAsSeenAsync(stroy.Value.Items[0].Id, 4);
                            dataAction.Response = "Story assistido com sucesso";
                        }
                        else
                        {
                            dataAction.Response = "Perfil não possui story";
                        }
                    } else
                    {
                        dataAction.Response = "Perfil não possui story";
                    }
                } else
                {
                    dataAction.Response = "Perfil não possui story";
                }
            } catch (Exception err)
            {
                dataAction.Response = $"Erro: {err.Message}";
                dataAction.Status = 0;
                LogMessage("Assistir Story erro - " + dataAction.Response);
            }
            return dataAction;
        }

        public async Task<ActionModel> DoHumanizationTaskOne(long pk)
        {
            ActionModel dataAction = new ActionModel
            {
                Type = "DoHumanizationTaskOne",
                Username = userSession.UserName,
                Status = 1,
                Response = ""
            };
            try
            {
                var stroy = await HelpersInstaApi.InstaApi.StoryProcessor.GetUserStoryAsync(pk);
                if (stroy.Succeeded)
                {
                    if (stroy.Value.Items != null)
                    {
                        if (stroy.Value.Items.Count > 0)
                        {
                            await HelpersInstaApi.InstaApi.StoryProcessor.MarkStoryAsSeenAsync(stroy.Value.Items[0].Id, 4);
                        }
                    }
                }
                var feed = await HelpersInstaApi.InstaApi.UserProcessor.GetFullUserInfoAsync(pk);
                if (feed.Succeeded)
                {
                    if (feed.Value.Feed.Items != null)
                    {
                        if (feed.Value.Feed.Items.Count > 0)
                        {
                            var r = new Random();
                            int c = feed.Value.Feed.Items.Count > 6 ? 5 : (feed.Value.Feed.Items.Count - 1);
                            if (c > 0)
                            {
                                await this.DoLike(feed.Value.Feed.Items[r.Next(0, c)].Pk);
                            }
                            else
                            {
                                await this.DoLike(feed.Value.Feed.Items[0].Pk);
                            }
                        }
                    }
                }
                dataAction.Response = "Humanização completa";
            }
            catch (Exception err)
            {
                dataAction.Response = $"Erro: {err.Message}";
                dataAction.Status = 0;
                LogMessage("Humanization one erro - " + dataAction.Response);
            }
            return dataAction;
        }

        public async Task<ActionModel> DoHumanizationTaskTwo(long pk)
        {
            ActionModel dataAction = new ActionModel
            {
                Type = "DoHumanizationTaskTwo",
                Username = userSession.UserName,
                Status = 1,
                Response = ""
            };
            try
            {
                var stroy = await HelpersInstaApi.InstaApi.StoryProcessor.GetUserStoryAsync(pk);
                if (stroy.Succeeded)
                {
                    if (stroy.Value.Items != null)
                    {
                        if (stroy.Value.Items.Count > 0)
                        {
                            await HelpersInstaApi.InstaApi.StoryProcessor.MarkStoryAsSeenAsync(stroy.Value.Items[0].Id, 4);
                        }
                    }
                }
                var feed = await HelpersInstaApi.InstaApi.UserProcessor.GetFullUserInfoAsync(pk);
                if (feed.Succeeded)
                {
                    if (feed.Value.Feed.Items != null)
                    {
                        if (feed.Value.Feed.Items.Count > 0)
                        {
                            var r = new Random();
                            int c = feed.Value.Feed.Items.Count > 6 ? 5 : (feed.Value.Feed.Items.Count - 1);
                            if (c > 0)
                            {
                                int id = r.Next(0, c);
                                await this.DoLike(feed.Value.Feed.Items[id].Pk);
                                await Task.Delay(TimeSpan.FromSeconds(r.Next(1, 6)));
                                await this.DoComment(feed.Value.Feed.Items[id].Pk, "🔥🔥");
                            }
                            else
                            {
                                await this.DoLike(feed.Value.Feed.Items[0].Pk);
                                await Task.Delay(TimeSpan.FromSeconds(r.Next(1, 6)));
                                await this.DoComment(feed.Value.Feed.Items[0].Pk, "🔥🔥");
                            }
                        }
                    }
                }
                dataAction.Response = "Humanização completa";
            }
            catch (Exception err)
            {
                dataAction.Response = $"Erro: {err.Message}";
                dataAction.Status = 0;
                LogMessage("Humanization two erro - " + dataAction.Response);
            }
            return dataAction;
        }

        public async Task<ActionModel> DoGetFeed(int type)
        {
            ActionModel dataAction = new ActionModel
            {
                Type = "DoGetFeed",
                Username = userSession.UserName,
                Status = 1,
                Response = ""
            };
            try
            {
                var r = new Random();
                if (type == 1)
                {
                    var feed = await HelpersInstaApi.InstaApi.FeedProcessor.GetFollowingRecentActivityFeedAsync(InstagramApiSharp.PaginationParameters.Empty);
                    if (feed.Succeeded)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(r.Next(2, 7)));
                    }
                } else
                {
                    if (type == 2)
                    {
                        var feed = await HelpersInstaApi.InstaApi.FeedProcessor.GetFollowingRecentActivityFeedAsync(InstagramApiSharp.PaginationParameters.Empty);
                        if (feed.Succeeded)
                        {
                            await Task.Delay(TimeSpan.FromSeconds(r.Next(2, 7)));
                            feed = await HelpersInstaApi.InstaApi.FeedProcessor.GetFollowingRecentActivityFeedAsync(InstagramApiSharp.PaginationParameters.Empty);
                            await Task.Delay(TimeSpan.FromSeconds(r.Next(2, 7)));
                        }
                    } else
                    {
                        var feed = await HelpersInstaApi.InstaApi.FeedProcessor.GetFollowingRecentActivityFeedAsync(InstagramApiSharp.PaginationParameters.Empty);
                        if (feed.Succeeded)
                        {
                            await Task.Delay(TimeSpan.FromSeconds(r.Next(2, 7)));
                            feed = await HelpersInstaApi.InstaApi.FeedProcessor.GetFollowingRecentActivityFeedAsync(InstagramApiSharp.PaginationParameters.Empty);
                            await Task.Delay(TimeSpan.FromSeconds(r.Next(2, 7)));
                            if (feed.Succeeded)
                            {
                                feed = await HelpersInstaApi.InstaApi.FeedProcessor.GetFollowingRecentActivityFeedAsync(InstagramApiSharp.PaginationParameters.Empty);
                                await Task.Delay(TimeSpan.FromSeconds(r.Next(2, 7)));
                            }
                        }
                    }
                }
                dataAction.Response = "Humanização completa";
            }
            catch (Exception err)
            {
                dataAction.Response = $"Erro: {err.Message}";
                dataAction.Status = 0;
                LogMessage("Assistir Story erro - " + dataAction.Response);
            }
            return dataAction;
        }

        public async Task SaveStateAccount ()
        {
            var dir = Directory.GetCurrentDirectory();
            string stateFile = $@"{dir}/Insta/{userSession.UserName}.bin";
            var state = HelpersInstaApi.InstaApi.GetStateDataAsString();
            // in .net core or uwp apps don't use GetStateDataAsStream.
            // use this one:
            // var state = _instaApi.GetStateDataAsString();
            // this returns you session as json string.
            if (File.Exists(stateFile))
            {
                File.Delete(stateFile);
            }
            File.WriteAllText(stateFile, state);
            await Task.Delay(500);
        }
    }
}
