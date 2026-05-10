using System.Diagnostics;
using MauiApp5.Models;
using SkiaSharp;
using SkiaSharp.Views.Maui;

namespace MauiApp5.Views;

public partial class GamePage
{
    private void OnCanvasPaintSurface(object sender, SKPaintSurfaceEventArgs e)
    {
        try
        {
            var canvas = e.Surface.Canvas;
            var info = e.Info;

            canvas.Clear(SKColors.Transparent);

            if (_board.Length == 0 || Size == 0) return;

            _cellWidth = (float)info.Width / Size;
            _cellHeight = (float)info.Height / Size;
        
            var baseNormalColor = SKColors.DarkSlateGray;
            var baseSelectedColor = _isErrorState ? SKColors.DarkRed : SKColors.Black;
            var baseSwapColor = SKColors.DarkTurquoise;
            var baseBorderColor = SKColors.White;
            var baseTextColor = SKColors.AntiqueWhite;
        
            using var sharedBgPaint = new SKPaint();
            sharedBgPaint.Style = SKPaintStyle.Fill;
            sharedBgPaint.IsAntialias = true;
            
            using var paintBorder = new SKPaint();
            paintBorder.Style = SKPaintStyle.Stroke;
            paintBorder.StrokeWidth = 2;
            paintBorder.IsAntialias = true;
            
            using var paintText = new SKPaint();
            paintText.IsAntialias = true;

            using var textFont = new SKFont();
            textFont.Size = Math.Min(_cellWidth, _cellHeight) * 0.5f;
            textFont.Typeface = _customFont;

            var textAlign = SKTextAlign.Center;

            textFont.MeasureText("A", out var textBounds);
            float textYOffset = -textBounds.MidY;
        
            float radiusX = 30;
            float radiusY = 30;
            
            for (int row = 0; row < Size; row++)
            {
                for (int col = 0; col < Size; col++)
                {
                    var cell = _board[row, col];
                    float x = (col * _cellWidth) + cell.OffsetX;
                    float floatY = (row * _cellHeight) + cell.OffsetY;
                
                    byte a = (byte)Math.Clamp(cell.Alpha, 0, 255);
                
                    SKColor bgColor = _selectedCells.Contains(cell) ? baseSelectedColor : baseNormalColor;
                    if (_swapFirstCell == cell) bgColor = baseSwapColor;
                
                    sharedBgPaint.Color = bgColor.WithAlpha(a);
                    paintBorder.Color = baseBorderColor.WithAlpha(a);
                    paintText.Color = baseTextColor.WithAlpha(a);
                
                    float cx = x + (_cellWidth / 2f);
                    float cy = floatY + (_cellHeight / 2f);

                    canvas.Save();
                
                    canvas.Scale(cell.Scale, cell.Scale, cx, cy);
                
                    canvas.DrawRoundRect(x, floatY, _cellWidth, _cellHeight, radiusX, radiusY, sharedBgPaint);
                    canvas.DrawRoundRect(x, floatY, _cellWidth, _cellHeight, radiusX, radiusY, paintBorder);

                    var powerColor = SKColors.Aquamarine;
                    
                    if (cell.Power != PowerType.None)
                    {
                        using var powerPaint = new SKPaint();
                        powerPaint.Color = powerColor.WithAlpha((byte)(a * 0.5f));
                        powerPaint.Style = SKPaintStyle.Stroke;
                        powerPaint.StrokeWidth = _cellWidth * 0.08f;
                        powerPaint.IsAntialias = true;
                        powerPaint.StrokeCap = SKStrokeCap.Round;

                        float padding = _cellWidth * 0.15f;
                        float spacing = _cellWidth * 0.20f;

                        switch (cell.Power)
                        {
                            case PowerType.ClearRow:
                                canvas.DrawLine(x + padding, cy - spacing, x + _cellWidth - padding, cy - spacing, powerPaint);
                                canvas.DrawLine(x + padding, cy, x + _cellWidth - padding, cy, powerPaint);
                                canvas.DrawLine(x + padding, cy + spacing, x + _cellWidth - padding, cy + spacing, powerPaint);
                                break;

                            case PowerType.ClearColumn:
                                canvas.DrawLine(cx - spacing, floatY + padding, cx - spacing, floatY + _cellHeight - padding, powerPaint);
                                canvas.DrawLine(cx, floatY + padding, cx, floatY + _cellHeight - padding, powerPaint);
                                canvas.DrawLine(cx + spacing, floatY + padding, cx + spacing, floatY + _cellHeight - padding, powerPaint);
                                break;

                            case PowerType.ClearNeighbors:
                                powerPaint.Style = SKPaintStyle.Fill;
                            
                                float diamondSize = _cellWidth * 0.55f;

                                canvas.Save();
                            
                                canvas.RotateDegrees(45, cx, cy);
                                
                                canvas.DrawRect(cx - diamondSize / 2, cy - diamondSize / 2, diamondSize, diamondSize, powerPaint);
                            
                                canvas.Restore();
                                break;

                            case PowerType.ClearBigArea:
                                powerPaint.Style = SKPaintStyle.Fill;
                                using (var starPath = CreateStarPath(cx, cy, _cellWidth * 0.45f, _cellWidth * 0.20f))
                                {
                                    canvas.DrawPath(starPath, powerPaint);
                                }
                                break;
                        }
                    }
                    
                    if (cell.Letter != '\0')
                    {
                        canvas.DrawText(cell.Letter.ToString(), cx, cy + textYOffset, textAlign, textFont,  paintText);
                    }
                
                    canvas.Restore();
                }
            }
        
            using var explosionPaint = new SKPaint();
            explosionPaint.Style = SKPaintStyle.Fill;
            explosionPaint.IsAntialias = true;

            lock (_explosions) 
            {
                foreach (var exp in _explosions)
                {
                    explosionPaint.Color = SKColors.Gray.WithAlpha((byte)exp.Alpha);
                    canvas.DrawCircle(exp.X, exp.Y, exp.Radius, explosionPaint);

                    explosionPaint.Color = SKColors.Black.WithAlpha((byte)(exp.Alpha * 0.6f));
                    canvas.DrawCircle(exp.X, exp.Y, exp.Radius * 0.6f, explosionPaint);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.ToString());
        }
    }
    
    private SKPath CreateStarPath(float cx, float cy, float outerRadius, float innerRadius)
    {
        var path = new SKPath();
        float angle = (float)(Math.PI / 2f);
        float angleStep = (float)(Math.PI / 5f);

        for (int i = 0; i < 10; i++)
        {
            float radius = (i % 2 == 0) ? outerRadius : innerRadius;
            float px = cx + (float)Math.Cos(angle) * radius;
            
            float py = cy - (float)Math.Sin(angle) * radius; 

            if (i == 0) path.MoveTo(px, py);
            else path.LineTo(px, py);

            angle -= angleStep;
        }
        path.Close();
        return path;
    }
}