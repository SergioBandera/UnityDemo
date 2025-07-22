using TMPro;
using UnityEngine;

public class UIScore : MonoBehaviour
{

    public TextMeshProUGUI myTextComponent;
    public int redTeamScore = 0;
    public int blueTeamScore = 0;
    public static UIScore Instance;
    public enum TeamType
    {
        RedTeam,
        BlueTeam,
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
    void Start()
    {
        UpdateScoreText();

    }

    // Update is called once per frame
    void Update()
    {

    }
    public void AddScore(TeamType team)
    {
        if (team == TeamType.RedTeam)
        {
            redTeamScore += 1;
        }
        else if (team == TeamType.BlueTeam)
        {
            blueTeamScore += 1;
        }

        UpdateScoreText();
    }
    public void UpdateScoreText()
    {
        if (myTextComponent != null)
        {
            myTextComponent.text = string.Format("{0}-{1}", redTeamScore, blueTeamScore);
        }
    }

    // para marcar solo hay que hacer esto comprobando que pase la linea de gol
    //UIScore.Instance.AddScore(UIScore.TeamType.BlueTeam);
    //UIScore.Instance.AddScore(UIScore.TeamType.RedTeam);
}
