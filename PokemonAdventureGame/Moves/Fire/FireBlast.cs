﻿using PokemonAdventureGame.Enums;
using PokemonAdventureGame.Interfaces;
using System.Collections.Generic;

namespace PokemonAdventureGame.Moves.Fire
{
    public class FireBlast : IMove
    {
        public Type Type { get => Type.FIRE; }
        public int Damage { get => 45; }
        public int PowerPoints { get; set; }
        public bool Special { get => true; }
        public List<StatusMove> StatusMoves { get => null; }
        public StatusMoveTarget? MoveTarget { get => null; }

        public FireBlast()
        {
            PowerPoints = 10;
        }
    }
}
