﻿using System;
using System.Threading;
using PokemonAdventureGame.Enums;
using PokemonAdventureGame.Interfaces;

namespace PokemonAdventureGame.BattleSystem
{
    public static class ConsoleBattleInfo
    {
        public static void ClearScreen()
            => Console.Clear();

        public static void EnemyTrainerSendsPokemon(ITrainer trainer, IPokemon pokemon)
        {
            WaitOneSecond();
            Console.WriteLine($"{trainer.GetType().Name} sent out {pokemon.GetType().Name}!");
            SkipLine();
        }

        private static void WaitOneSecond() => Thread.Sleep(1000);
        private static void WaitTwoSeconds() => Thread.Sleep(2000);

        public static void PlayerSendsPokemon(IPokemon pokemon)
        {
            WaitOneSecond();
            Console.WriteLine($"Go, {pokemon.GetType().Name}!");
            WaitOneSecond();
            ClearScreen();
        }

        public static void TrainerDrawsbackPokemon(IPokemon pokemon) => Console.WriteLine($"{pokemon.GetType().Name}, come back!");

        public static void SkipLine() => Console.WriteLine(string.Empty);

        public static void ShowBothPokemonStats(IPokemon playerPokemon, IPokemon enemyPokemon)
        {
            //Show status ailments in the future in front of the Pokémon's HP.
            Console.WriteLine($"{enemyPokemon.GetType().Name} - HP: {enemyPokemon.CurrentHealthPoints}/{enemyPokemon.HealthPoints}");
            SkipLine();
            Console.WriteLine($"{playerPokemon.GetType().Name} - HP: {playerPokemon.CurrentHealthPoints}/{playerPokemon.HealthPoints}");
            SkipLine();
        }

        public static void ShowAvailableCommandsOnConsole()
        {
            Console.WriteLine($"{(int)Command.ATTACK}: {Command.ATTACK.ToString()}");
            Console.WriteLine($"{(int)Command.SWITCH_POKEMON}: {Command.SWITCH_POKEMON.ToString().Replace("_", " ")}");
            Console.WriteLine($"{(int)Command.ITEMS}: {Command.ITEMS.ToString()}");
            Console.WriteLine($"{(int)Command.RUN}: {Command.RUN.ToString()}");
        }

        public static void ShowPokemonUsedMove(IPokemon pokemon, string moveName)
        {
            Console.WriteLine($"{pokemon.GetType().Name} used {moveName}!");
            WaitTwoSeconds();
            SkipLine();
        }

        public static void ShowPokemonReceivedDamage(IPokemon pokemon, int damage)
        {
            Console.WriteLine($"{pokemon.GetType().Name} took {damage} damage!");
            WaitTwoSeconds();
            SkipLine();
        }

        public static void WriteAllAvailableAttacksOnConsole(IPokemon pokemon)
        {
            SkipLine();
            Console.WriteLine("Choose your attack!");
            SkipLine();

            for (int i = 0; i < pokemon.Moves.Count; i++)
                Console.WriteLine($"{i}: {pokemon.Moves[i].GetType().Name} | PP: {pokemon.Moves[i].PowerPoints}");
        }

        public static int GetPlayerChosenMove(string userInput)
        {
            ClearScreen();
            return int.TryParse(userInput, out int chosenMove) ? chosenMove : -1;
        }

        public static void ShowPokemonFainted(IPokemon pokemon)
        {
            Console.WriteLine($"{pokemon.GetType().Name} fainted!");
            SkipLine();
        }

        public static void ShowTrainerWins(ITrainer trainer)
        {
            Console.WriteLine($"{trainer.GetType().Name} wins!");
            SkipLine();
        }

        public static void TrainerHasNoPokemonLeft(ITrainer trainer)
        {
            Console.WriteLine($"{trainer.GetType().Name} has no other pokemon left to battle...");
            SkipLine();
        }

        public static void MovementDidntAffectPokemon(IPokemon pokemon)
        { 
            Console.WriteLine($"It didn't affect {pokemon.GetType().Name}!");
            WaitOneSecond();
        }

        private static void MovementIsNotVeryEffective()
            => Console.WriteLine("It's not very effective...");

        private static void MovementIsSuperEffective()
            => Console.WriteLine("It's super effective!");

        public static void ShowHowEffectiveTheMoveWas(TypeEffect typeEffect, IPokemon pokemon)
        {
            switch (typeEffect)
            {
                case TypeEffect.IMMUNE:
                    MovementDidntAffectPokemon(pokemon);
                    break;
                case TypeEffect.NOT_VERY_EFFECTIVE:
                    MovementIsNotVeryEffective();
                    WaitOneSecond();
                    break;
                case TypeEffect.SUPER_EFFECTIVE:
                    MovementIsSuperEffective();
                    WaitOneSecond();
                    break;
            }
        }
        public static void MovementIsOutOfPowerPoints() 
            => Console.WriteLine("The chosen move is out of Power Points!");
    }
}