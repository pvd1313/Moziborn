using UnityEngine;
using UnityEngine.Tilemaps;

namespace ProjectTolerance.Buildings
{
    public class LevelView : MonoBehaviour
    {
        public Tilemap floor0Frame => _floor0Frame;
        public Tilemap floor0Interactive => _floor0Interactive;
        public Tilemap floor1Frame => _floor1Frame;
        
        [SerializeField] private Tilemap _floor0Frame;
        [SerializeField] private Tilemap _floor0Interactive;
        [SerializeField] private Tilemap _floor1Frame;
    }
}