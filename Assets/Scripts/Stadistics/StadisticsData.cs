using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StadisticsData
{
    [Serializable]
    public class Stadistics{
        public string gameId;
        public string playerId;
        public int level;
        public string dataType;
        public string FechaInterna;
        [SerializeField] public DataClass data;

        public Stadistics(string type){
            this.gameId = GameManager.instance.gameId;
            this.playerId = GameManager.instance.playerData.playerID;
            this.level = GameManager.instance.currentStation;
            this.dataType = type;
            this.FechaInterna = DateTime.Now.ToString();
        }
    }

    [Serializable]
    public class DataSpecie : DataClass{
        public string specieId;
        public string show_desc_date;

        public DataSpecie(string id){
            this.specieId = id;
            this.show_desc_date = DateTime.Now.ToString();
        }
    }

    [Serializable]
    public class DataGame : DataClass{
        public string Start_Date;
        public string End_Date;
        public int duration;
        public int score;
        public int NumberImages;

        public DataGame(DateTime start, int duration,int score, int num){
            this.Start_Date = start.ToString();
            this.End_Date = DateTime.Now.ToString();
            this.duration = duration;
            this.score = score;
            this.NumberImages = num;
        }
    }

    [Serializable]
    public class DataMission : DataClass{
        public string Start_Date;
        public string End_Date;
        public string nombre;

        public DataMission(DateTime start, string name){
            this.Start_Date = start.ToString();
            this.End_Date = DateTime.Now.ToString();
            this.nombre = name;
        }
    }

    [Serializable]
    public class DataPrize : DataClass{
        public string nombre;
        public string date;

        public DataPrize(string name){
            this.nombre = name;
            this.date = DateTime.Now.ToString();
        }
    }

    [Serializable]
    public class DataExperiencie : DataClass{
        public int experiencia;
        public string date;

        public DataExperiencie(int exp){
            this.experiencia = exp;
            this.date = DateTime.Now.ToString();
        }
    }

    [Serializable]
    public class DataJumps : DataClass {
        public int totalJumps;
        public string date;

        public DataJumps(int jumps) {
            this.totalJumps = jumps;
            this.date = DateTime.Now.ToString();
        }
    }

    [Serializable]
    public class DataClass{
        
    }
    public List<Stadistics> lista;
}
