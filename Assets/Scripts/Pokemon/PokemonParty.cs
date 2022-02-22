using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PokemonParty : MonoBehaviour
{
    // トレーナーのポケモンを管理する
    [SerializeField] List<Pokemon> pokemons;

    public List<Pokemon> Pokemons { get => pokemons; }

    private void Start()
    {
        foreach (Pokemon pokemon in Pokemons)
        {
            pokemon.Init();
        }
    }

    // 戦えるポケモンを渡す（HP>0のぽけもんを返す）
    public Pokemon GetHealthyPokemon()
    {
        return Pokemons.Where(monster => monster.HP > 0).FirstOrDefault();
    }
}
