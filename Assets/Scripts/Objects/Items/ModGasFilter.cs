using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts;
using Assets.Scripts.Atmospherics;
using Assets.Scripts.Objects.Items;
using Assets.Scripts.Objects.Clothing;
using zach2039.CustomGasMod.Assets.Scripts.Atmospherics;

namespace zach2039.CustomGasMod.Assets.Scripts.Objects.Items
{
    public class ModGasFilter : GasFilter
    {
		public float FilterGas(ref ModGasMixture fromMix, ref ModGasMixture tooMix, Atmosphere atmosphere, float minRatio)
		{
			if (base.Quantity <= 0f)
			{
				this.HandleEmptyFilter(this.ParentSuit);
				return 0f;
			}
			float num = ModAtmosphere.FilterGas(this.FilterType, ref fromMix, ref tooMix, atmosphere, minRatio);
			if (num > 0f)
			{
				this._usedTicks++;
				this.CheckUsedTicks();
			}
			return num;
		}

		public void FilterGas(ref ModGasMixture fromMix, ref ModGasMixture tooMix)
		{
			if (base.Quantity <= 0f)
			{
				this.HandleEmptyFilter(this.ParentSuit);
				return;
			}
			if (ModAtmosphere.FilterGas(this.FilterType, ref fromMix, ref tooMix))
			{
				this._usedTicks++;
				this.CheckUsedTicks();
			}
		}

		private void CheckUsedTicks()
		{
			if (!GameManager.RunSimulation)
			{
				return;
			}
			if (this._usedTicks > (int)this.TicksBeforeDegrade)
			{
				base.Quantity -= 1f;
				this._usedTicks = 0;
			}
		}

		private void HandleEmptyFilter(Suit suit)
		{
			if (suit)
			{
				suit.UpdateEmptyFilter();
			}
		}

		private Suit ParentSuit
		{
			get
			{
				if (this.ParentSlot == null)
				{
					return null;
				}
				return this.ParentSlot.Parent as Suit;
			}
		}

		public new int FilterType { get; set; }

		private int _usedTicks;
	}
}