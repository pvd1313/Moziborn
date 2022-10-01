using System.Collections;
using ProjectTolerance.Buildings;
using UnityEngine;

namespace ProjectTolerance
{
    public class GameLogic : MonoBehaviour
    {
        [SerializeField] private BuildingController _buildingController;
        [SerializeField] private AmbientLightController _ambientLightController;

        private IEnumerator Start()
        {
            yield return _buildingController.Initialize();
            yield return _ambientLightController.Initialize();
        }
    }
}