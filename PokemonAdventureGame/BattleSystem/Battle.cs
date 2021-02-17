﻿using System;
using System.Linq;
using System.Collections.Generic;
using PokemonAdventureGame.Enums;
using PokemonAdventureGame.Types;
using PokemonAdventureGame.Interfaces;
using PokemonAdventureGame.PokemonTeam;
using PokemonAdventureGame.BattleSystem.ConsoleUI;

namespace PokemonAdventureGame.BattleSystem
{
    public class Battle
    {
        #region Private Properties

        private delegate bool PokemonAttackDelegate();
        private delegate bool SwitchPokemonDelegate();
        private delegate bool UseItemDelegate();
        private const int LIMIT_OF_MOVES_PER_POKEMON = 4;
        private ITrainer Player { get; set; }
        private ITrainer EnemyTrainer { get; set; }
        private BattleAux BattleAux { get; set; }
        private Dictionary<Command, Delegate> Commands { get; set; }

        #endregion

        public Battle(ITrainer player, ITrainer enemyTrainer)
        {
            Player = player;
            EnemyTrainer = enemyTrainer;
            BattleAux = new BattleAux(player, enemyTrainer);
            InitializeCommandDictionary();
        }

        private void InitializeCommandDictionary()
        {
            PokemonAttackDelegate firstMethodForAttackDelegate = PromptTrainerForPokemonMove;
            SwitchPokemonDelegate switchPokemonDelegate = PromptPlayerToSelectPokemon;
            UseItemDelegate chooseItemDelegate = PromptPlayerToChooseItem;

            Commands = new Dictionary<Command, Delegate>
            {
                { Command.ATTACK, firstMethodForAttackDelegate },
                { Command.SWITCH_POKEMON, switchPokemonDelegate },
                { Command.ITEMS, chooseItemDelegate }
            };
        }

        public bool StartBattle()
        {
            BothTrainersSendPokemon();
            return KeepBattleGoingWhileBothPlayersHavePokemonLeft();
        }

        private void BothTrainersSendPokemon()
        {
            Player.SetPokemonAsCurrent(Player.GetNextAvailablePokemon());
            EnemyTrainer.SetPokemonAsCurrent(EnemyTrainer.GetNextAvailablePokemon());

            EnemyAction.EnemyTrainerSendsPokemon(EnemyTrainer);
            PlayerAction.PlayerSendsPokemon(Player.GetCurrentPokemon());
        }

        private bool KeepBattleGoingWhileBothPlayersHavePokemonLeft()
        {
            bool playerWon = false;
            while (Player.HasAvailablePokemon() && EnemyTrainer.HasAvailablePokemon())
            {
                bool keepBattleGoing = false, isChangingToNextAvailablePokemon = false;

                if (Player.GetCurrentPokemon().CurrentHealthPoints <= 0 && EnemyTrainer.HasAvailablePokemon())
                {
                    PromptPlayerToSelectPokemonAfterAnotherPokemonFainted();
                    isChangingToNextAvailablePokemon = true;
                }
                else
                {
                    ConsoleBattleInfoPokemon.ShowBothPokemonStats(Player.GetCurrentPokemon(), EnemyTrainer.GetCurrentPokemon());
                    keepBattleGoing = PlayerMove();
                }

                if (keepBattleGoing && !isChangingToNextAvailablePokemon)
                {
                    if (EnemyTrainer.GetCurrentPokemon().HasFainted() && Player.HasAvailablePokemon())
                    {
                        if (BattleAux.CannotSendNextAvailablePokemon(EnemyTrainer, isEnemyTrainer: true))
                            break;
                        else
                            PromptPlayerToSelectPokemonAfterAnotherPokemonFainted();
                    }
                    else
                        EnemyMove();
                }
            }

            if (Player.HasAvailablePokemon())
                playerWon = true;

            return playerWon;
        }

        private bool PlayerMove()
        {
            Console.WriteLine("What are you going to do next?");
            ConsoleBattleInfo.ShowAvailableCommandsOnConsole();

            Command command = (Command)Enum.Parse(typeof(Command), Console.ReadLine() ?? "1");
            bool keepBattleGoing = (bool)Commands[command].DynamicInvoke();

            return keepBattleGoing;
        }

        #region Player Commands

        private bool PromptTrainerForPokemonMove()
        {
            bool keepBattleGoing = true;
            int chosenMove = BattleAux.KeepPlayerChoosingMove(LIMIT_OF_MOVES_PER_POKEMON);
            PokemonAttack(Player.GetCurrentPokemon(), EnemyTrainer.GetCurrentPokemon(), chosenMove);

            return keepBattleGoing;
        }

        private void PokemonAttack(IPokemon attackingPokemon, IPokemon targetPokemon, int chosenMove)
        {
            IMove move = attackingPokemon.Moves[chosenMove];

            if (ConsoleBattleInfoMove.MoveDoesNotHavePowerPointsLeft(move))
            {
                PromptTrainerForPokemonMove();
                return;
            }

            ConsoleBattleInfoPokemon.ShowPokemonUsedMove(attackingPokemon, move.GetType().Name);

            if (TypeComparer.PokemonTypeDoesNotMakeContactWithMove(targetPokemon.Types, move))
                ConsoleUtils.ShowMessageAndWaitOneSecond($"It didn't affect {targetPokemon.GetType().Name}!");
            else
                ProcessAttack(attackingPokemon, targetPokemon, move);

            ConsoleUtils.ClearScreen();
        }

        private static void ProcessAttack(IPokemon attackingPokemon, IPokemon targetPokemon, IMove move)
        {
            TypeEffect moveEffectOnPokemon = TypeComparer.GetMoveEffectivenessBasedOnPokemonType(move.Type, targetPokemon.Types.FirstOrDefault());

            int calculatedDamage = TypeDamageCalculator.CalculateDamage(attackingPokemon, targetPokemon, move, moveEffectOnPokemon);

            attackingPokemon.UseMove(move);

            if (move.StatusMoves != null) 
            {
                BattleAux.ProcessStatusAttack(attackingPokemon, targetPokemon, move);
			}
            else
            {
                targetPokemon.ReceiveDamage(calculatedDamage);
                ConsoleBattleInfoPokemon.ShowPokemonReceivedDamage(targetPokemon, calculatedDamage);
                ConsoleBattleInfoTypes.ShowHowEffectiveTheMoveWas(moveEffectOnPokemon);
            }
        }

        private bool PromptPlayerToSelectPokemon()
        {
            bool keepBattleGoingAfterPokemonSelection = false;

            if (Player.PokemonTeam.Where(pkmn => !pkmn.Fainted).Count() == 1)
            {
                ConsoleBattleInfoTrainer.ShowPlayerThereAreNoPokemonLeftToSwitch();
                return keepBattleGoingAfterPokemonSelection;
            }

            int chosenPokemonIndex = BattleAux.KeepPlayerChoosingPokemonIndex();

            SwitchCurrentPokemon(chosenPokemonIndex);
            keepBattleGoingAfterPokemonSelection = true;

            return keepBattleGoingAfterPokemonSelection;
        }

        private void SwitchCurrentPokemon(int chosenPokemon, bool isChangingAfterOwnPokemonFainted = false)
        {
            TrainerPokemon pokemon = Player.PokemonTeam[chosenPokemon];

            if (pokemon is null || (pokemon.Current && isChangingAfterOwnPokemonFainted))
                return;

            if (pokemon.Fainted)
            {
                ConsoleBattleInfoPokemon.ShowChosenPokemonIsNotAvailable();
                PlayerMove();
                return;
            } 
            else if (pokemon.Current)
            {
                ConsoleBattleInfoPokemon.ShowChosenPokemonIsAlreadyInBattle();
                PlayerMove();
                return;
            }

            BattleAux.DrawbackThenSendPokemon(chosenPokemon);
        }

        private bool PromptPlayerToChooseItem()
        {
            int chosenStackedItemsIndex = BattleAux.KeepPlayerChoosingItem(Player, Player.Items.Count);
            int chosenPokemonIndex = BattleAux.KeepPlayerChoosingPokemonIndex();

            IPokemon chosenPokemon = Player.PokemonTeam[chosenPokemonIndex].Pokemon;
            IItem chosenItem = Player.Items.ElementAt(chosenStackedItemsIndex).Value.FirstOrDefault();

            return CheckIfShouldKeepBattleGoingAfterItemSelection(chosenItem, chosenStackedItemsIndex, chosenPokemon);
        }

        private bool CheckIfShouldKeepBattleGoingAfterItemSelection(IItem chosenItem, int chosenStackedItemsIndex, IPokemon chosenPokemon)
        {
            bool itemWasSuccessfullyUsed = false;
            if (chosenItem != null && chosenItem.TryToUseItemOnPokemon(chosenPokemon))
            {
                ConsoleBattleInfoItems.ShowItemWasUsedOnPokemon(chosenItem, chosenPokemon);
                Player.Items.ElementAt(chosenStackedItemsIndex).Value.Remove(chosenItem);
                itemWasSuccessfullyUsed = true;
            }
            else
                ConsoleBattleInfoItems.ShowItemCannotBeUsed();

            return itemWasSuccessfullyUsed;
        }

        private void PromptPlayerToSelectPokemonAfterAnotherPokemonFainted()
        {
            if (Player.PokemonTeam.Count > 1 && Player.PokemonTeam.Where(w => !w.Pokemon.HasFainted()).Count() > 1)
            {
                ConsoleUtils.ShowMessageBetweenEmptyLines("Which pokemon will you choose?");
                int chosenPokemonIndex = BattleAux.KeepPlayerChoosingPokemonIndex();

                SwitchCurrentPokemon(chosenPokemonIndex, isChangingAfterOwnPokemonFainted: true);
            }
        }

        #endregion

        private void EnemyMove()
        {
            IPokemon enemyPokemon = EnemyTrainer.GetCurrentPokemon();
            int randomEnemyMoveIndex = new Random().Next(0, enemyPokemon.Moves.Count);

            PokemonAttack(enemyPokemon, Player.GetCurrentPokemon(), randomEnemyMoveIndex);
        }
    }
}
