namespace BookStoreManagement.UI.Components;

// White panel with a thin border (looks like a card).
public class CardPanel : Panel
{
    public CardPanel()
    {
        BackColor = Color.White;
        SetStyle(ControlStyles.ResizeRedraw, true);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        using Pen pen = new Pen(Theme.Border);
        e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
    }
}
