using System;
using UnityEngine;
using UnityEngine.Events;

namespace AresResurgence.Interaction
{
    /// <summary>
    /// Interactive console, terminal, button, or artifact in the game world.
    /// </summary>
    public class ConsoleInteractable : MonoBehaviour, IInteractable
    {
        [Header("Interaction Settings")]
        [SerializeField] private string promptText = "Interact";
        [SerializeField] private bool canInteract = true;
        [SerializeField] private bool singleUse = false;
        [SerializeField] private Color highlightColor = new Color(0.2f, 0.8f, 1f, 1f);

        [Header("Feedback")]
        [SerializeField] private Light statusIndicatorLight;
        [SerializeField] private Color activeColor = Color.green;
        [SerializeField] private Color inactiveColor = Color.red;

        [Header("Events")]
        public UnityEvent onInteracted;

        private bool hasBeenUsed = false;
        private Renderer objectRenderer;
        private Material originalMaterial;

        public string PromptText => promptText;
        public bool CanInteract => canInteract && (!singleUse || !hasBeenUsed);

        private void Awake()
        {
            objectRenderer = GetComponentInChildren<Renderer>();
            if (statusIndicatorLight != null)
            {
                statusIndicatorLight.color = CanInteract ? activeColor : inactiveColor;
            }
        }

        public void SetPrompt(string newPrompt)
        {
            promptText = newPrompt;
        }

        public void SetInteractable(bool active)
        {
            canInteract = active;
            if (statusIndicatorLight != null)
            {
                statusIndicatorLight.color = CanInteract ? activeColor : inactiveColor;
            }
        }

        public void Interact(GameObject interactor)
        {
            if (!CanInteract) return;

            hasBeenUsed = true;
            onInteracted?.Invoke();

            if (statusIndicatorLight != null)
            {
                statusIndicatorLight.color = CanInteract ? activeColor : inactiveColor;
            }
        }
    }
}
