using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuASM
{
    /// <summary>
    /// A batch of commands that happen at the specified timestamp, not including the <see cref="Commands.Command_TIME"/> command.
    /// </summary>
    struct TimestampedBatch
    {
        public UInt32 Timestamp;
        public List<TimestampedCommand> Commands;
    }

    /// <summary>
    /// A command that occurs at a specific time
    /// </summary>
    internal struct TimestampedCommand
    {
        /// <summary>
        /// The command that occurs
        /// </summary>
        public DSCCommand command;
        /// <summary>
        /// The time when it occurs
        /// </summary>
        public UInt32 timestamp;
        /// <summary>
        /// The branch in which it occurs
        /// </summary>
        public Int32 branch;
    }

    /// <summary>
    /// A collection of tools for sorting the script commands based upon timestamps.
    /// NB! This currently doesn't support PV_BRANCH_MODE properly and will likely break scripts that use it.
    /// </summary>
    static class TimestampBatchSorter
    {

        /// <summary>
        /// Sorts all the commands provided into timestamped bins. 
        /// Input data must be e.g. a concatenated script, as in: Time command followed by other commands is considered a block.
        /// </summary>
        /// <param name="list">List of commands to sort</param>
        /// <returns>Command lists in batches grouped by occurrence time</returns>
        public static List<TimestampedBatch> BinItemsFromList(List<DSCCommand> list)
        {
            List<TimestampedCommand> commands = new List<TimestampedCommand>();

            UInt32 currentTime = 0;
            Int32 currentBranch = 0;

            // timestamp all commands based on the time commands, and exclude time commands themselves
            for (int i = 0; i < list.Count; i++)
            {
                DSCCommand cmd = list[i];
                if (cmd.GetType() == typeof(Commands.Command_TIME))
                {
                    currentTime = ((Commands.Command_TIME)cmd).Timestamp;
                }
                else if (cmd.GetType() == typeof(Commands.Command_PV_BRANCH_MODE))
                {
                    currentBranch = ((Commands.Command_PV_BRANCH_MODE)cmd).branch;
                }
                else
                {
                    commands.Add(new TimestampedCommand() { timestamp = currentTime, command = cmd, branch = currentBranch });
                }
            }


            // Bin all batches with the same timestamp
            var binned = commands.OrderBy(c => c.timestamp)
                .GroupBy(c => c.timestamp)
                .Select(arr => new TimestampedBatch { Timestamp = arr.First().timestamp, Commands = arr.ToList() })
                .ToList();

            return binned;
        }


        /// <summary>
        /// Sorts a script to fix timing sequence, e.g. after a concatenation. <seealso cref="BinItemsFromList(List{DSCCommand})"/>
        /// </summary>
        /// <param name="list">Script to sort</param>
        /// <returns>Sorted script</returns>
        public static List<DSCCommand> SortedCommandList(List<DSCCommand> list)
        {
            List<DSCCommand> sortedCommands = new List<DSCCommand>();
            var bins = BinItemsFromList(list);
            Int32 currentBranch = -1;
            foreach (var bin in bins)
            {
                sortedCommands.Add(new Commands.Command_TIME(bin.Timestamp));
                foreach(var command in bin.Commands)
                {
                    if(command.branch != currentBranch)
                    {
                        sortedCommands.Add(new Commands.Command_PV_BRANCH_MODE(command.branch));
                        currentBranch = command.branch;
                    }
                    sortedCommands.Add(command.command);
                }
            }
            return sortedCommands;
        }
    }
}
