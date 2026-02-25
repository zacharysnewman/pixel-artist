using System.Collections.Generic;
using UnityEngine;

namespace PixelArtist.Canvas
{
    /// <summary>
    /// Full-snapshot undo/redo for a PixelCanvas.
    /// Push a snapshot before any mutation; call Undo/Redo to travel through history.
    /// Stack is capped at MaxSteps to bound memory.
    /// </summary>
    public class UndoSystem
    {
        public const int MaxSteps = 50;

        readonly Stack<Color32[]> _undoStack = new Stack<Color32[]>();
        readonly Stack<Color32[]> _redoStack = new Stack<Color32[]>();

        public bool CanUndo => _undoStack.Count > 0;
        public bool CanRedo => _redoStack.Count > 0;

        /// <summary>
        /// Call BEFORE mutating the canvas to record its current state.
        /// Clears the redo stack.
        /// </summary>
        public void Push(PixelCanvas canvas)
        {
            _undoStack.Push(canvas.Snapshot());
            _redoStack.Clear();

            // Trim to cap
            if (_undoStack.Count > MaxSteps)
            {
                // Stack doesn't support direct trimming; rebuild without the oldest entry.
                var temp = new Color32[_undoStack.Count][];
                _undoStack.CopyTo(temp, 0);
                _undoStack.Clear();
                for (int i = MaxSteps - 1; i >= 0; i--)
                    _undoStack.Push(temp[i]);
            }
        }

        public void Undo(PixelCanvas canvas)
        {
            if (!CanUndo) return;
            _redoStack.Push(canvas.Snapshot());
            canvas.Restore(_undoStack.Pop());
        }

        public void Redo(PixelCanvas canvas)
        {
            if (!CanRedo) return;
            _undoStack.Push(canvas.Snapshot());
            canvas.Restore(_redoStack.Pop());
        }

        public void Clear()
        {
            _undoStack.Clear();
            _redoStack.Clear();
        }
    }
}
