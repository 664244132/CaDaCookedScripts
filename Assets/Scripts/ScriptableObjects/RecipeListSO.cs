using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;

//[CreateAssetMenu()]
public class RecipeListSO : ScriptableObject
{
    // ใช้ FormerlySerializedAs เพื่อรักษาข้อมูลที่เซฟไว้ใน _RecipeListSO.asset ให้ยังคงโหลดได้ 100%
    [FormerlySerializedAs("recipeSOLsit")]
    public List<RecipeSO> recipeSOList;

    // Property สำหรับความเข้ากันได้ย้อนหลัง (Backward Compatibility)
    public List<RecipeSO> recipeSOLsit => recipeSOList;
}
