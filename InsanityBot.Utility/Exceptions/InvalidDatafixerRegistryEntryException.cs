﻿namespace InsanityBot.Utility.Exceptions;
using System;

public class InvalidDatafixerRegistryEntryException : Exception
{
	public Type Datafixable { get; set; }
	public Type Datafixer { get; set; }

	public InvalidDatafixerRegistryEntryException(Type Datafixable, Type Datafixer, String Message) : base(Message)
	{
		this.Datafixable = Datafixable;
		this.Datafixer = Datafixer;
	}
}