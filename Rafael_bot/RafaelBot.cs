using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Net;
using Discord.Commands;
using Discord;
using System.Collections;

namespace Rafael_bot
{
    enum Roles { };
    public class RafaelBot
    {

        public static DiscordClient discord;

        // variables for a poll
        List<string> answers = new List<string>();
        int answerPosition;
        string pollResults;
        string pollQuestion;
        bool isPollCreated;
        bool isPollStarted;
        bool isPollEnded;
        int[] answerVotes;
        User pollCreator;

        public RafaelBot()
        {
            discord = new DiscordClient(x =>
            {
                x.LogLevel = LogSeverity.Info;
                x.LogHandler = log;
            });

            discord.UsingCommands(x =>
            {
                x.PrefixChar = '!';
                x.AllowMentionPrefix = true;
            });

            discord.ExecuteAndWait(async () =>
            {
                await discord.Connect("MjM3MzIyNDAwNDE5Njc2MTYw.CxEAFA.2yBagzajJX14EQKFArkkNlwUdm4", TokenType.Bot);

            });

            var commands = discord.GetService<CommandService>();

            
            commands.CreateCommand("confess")
                .Alias(new string[] { "confession" })
                .Parameter("x", ParameterType.Unparsed)
                .Do(async (e) =>

                {
                    Message[] Messages;
                    Messages = await e.Channel.DownloadMessages(1);
                    await e.Channel.DeleteMessages(Messages);

                    await e.Server.GetChannel(249277412020322304).SendMessage(e.GetArg("x"));
                });
            commands.CreateCommand("test")
                .Alias(new string[] { "tests" })
                .Parameter("x", ParameterType.Unparsed)
                .Do(async (e) =>

                {
                    await e.Channel.SendMessage(e.Server.FindRoles("RoleOne").FirstOrDefault().Id.ToString());
                });

            commands.CreateGroup("poll", cgb =>
            {
                cgb.CreateCommand("create").
                    Parameter("x", ParameterType.Unparsed)
                    .Do(async (e) =>
                {
                    if (!isPollCreated)
                    {
                        pollCreator = e.Message.User;
                        Console.WriteLine("poll created");
                        isPollCreated = true;
                        isPollStarted = false;
                        answerPosition = 0;
                        pollQuestion = e.GetArg("x");
                        await e.Channel.SendMessage("A poll has been created with question: " + pollQuestion);
                    }
                    else
                    {
                        commandDenied(e.Channel);
                    }
                });

                cgb.CreateCommand("answer").
                    Parameter("x", ParameterType.Unparsed).
                    Do(async (e) =>
                    {
                    if (isPollCreated && !isPollStarted && pollCreator == e.Message.User)
                        {
                            string answer = e.GetArg("x");
                            Console.WriteLine("Answer has been added");
                            addAnswerToList(answer);
                            await e.Channel.SendMessage("The answer: *" + answers[answerPosition-1] + "* Has been assigned to answer[" + answerPosition + "]");
                        }
                        else if(isPollCreated && !isPollStarted && pollCreator != e.Message.User)
                        {
                            await e.Channel.SendMessage("You're not the one that has created the poll");
                        }
                        else
                        {
                            commandDenied(e.Channel);
                        }
                    });
                cgb.CreateCommand("start").
                Do(async (e) =>
                {
                    if (isPollCreated && answers.Count >= 2 && !isPollStarted && pollCreator == e.Message.User)
                    {
                        answerVotes = new int[answers.Count];
                        isPollStarted = true;
                        isPollEnded = false;
                        await e.Channel.SendMessage("The poll has started, voting can begin");
                    }
                    else if (isPollCreated && !isPollStarted && pollCreator != e.Message.User)
                    {
                            await e.Channel.SendMessage("You're not the one that has created the poll");
                    }
                    else if (isPollCreated)
                    {
                        await e.Channel.SendMessage("You need at least 2 answers");
                    }
                    else
                    {
                        commandDenied(e.Channel);
                    }
                });
                cgb.CreateCommand("vote").
                Parameter("x", ParameterType.Required).
                Do(async (e) =>
                {
                    int answerNumber;
                    int.TryParse(e.GetArg("x"), out answerNumber);
                    
                    if (isPollStarted)
                    {
                        if(answerNumber <= answers.Count)
                        {
                            answerVotes[answerNumber-1]++;
                            await e.Channel.SendMessage("A vote has been added to answer[" + answerNumber + "] *" + answers[answerNumber-1] + "*");
                            await e.Channel.SendMessage("with a total votes of: " + answerVotes[answerNumber-1]);

                        }
                        else
                        {
                            await e.Channel.SendMessage("that's not a valid number fucker");
                        }
                    }
                    else
                    {
                        commandDenied(e.Channel);
                    }
                });
                cgb.CreateCommand("end").
                Do(async (e) =>
                {
                    if (pollCreator == e.Message.User /*|| e.User.HasRole()*/)
                    {
                        pollResults = "";
                        isPollEnded = true;
                        isPollStarted = false;
                        isPollCreated = false;
                        pollResults = "The poll " + pollQuestion +" has ended, here are the results:\n\n";                       
                        for (int i = 0; i < answers.Count; i++)
                        {
                            pollResults += "Answer [" + i + "] *" + answers[i] + "*: with " + answerVotes[i] + " votes! \n";
                        }
                        await e.Channel.SendMessage(pollResults);
                        
                        answers.Clear();
                        Array.Clear(answerVotes,0,answerVotes.Length);
                    }
                    else
                    {
                        commandDenied(e.Channel);
                    }
                });
                
            });





        

        }
        //end of constructor RafaelBot

        private void log(object sender, LogMessageEventArgs e)
        {
            Console.WriteLine(e.Message);
        }

        private void addAnswerToList(string answer)
        {
            answers.Add(answer);
            answerPosition++;
        }

        private void commandDenied(Channel channel)
        {
            channel.SendMessage("you cannot use that command right now.");   
        }


    }
}
