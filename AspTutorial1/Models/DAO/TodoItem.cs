using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AspTutorial1.Models;

// 모델은 데이터베이스에 접근하는데 사용되는 데이터 개체를 말한다.
public class TodoItem
{
    public long Id { get; set; }
    public string Name { get; set; }
    public bool IsComplete { get; set; }
    public string Secret { get; set; }
}