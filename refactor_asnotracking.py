import os
import re

app_dir = r"d:\Code\TechGearAuction\TechGearAuction.Application\Features"

for root, _, files in os.walk(app_dir):
    for filename in files:
        if filename.endswith("Query.cs") or filename.endswith("Queries.cs"):
            filepath = os.path.join(root, filename)
            with open(filepath, 'r', encoding='utf-8') as f:
                content = f.read()

            # We want to add .AsNoTracking() to LINQ queries
            # typically right after `_context.DbSet` (e.g. `_context.Auctions`, `_context.Users`, etc.)
            # Wait, it could be `_context.Auctions.Include(...)`
            # We can use regex to find `_context\.[A-Za-z]+(\s*\n\s*\.|\.)` and insert `.AsNoTracking()` if not present.
            
            # Simple approach: find all `_context.[A-Za-z]+` followed by something, and replace it with `_context.\1.AsNoTracking()`
            # BUT we need to avoid adding it multiple times, or to things that aren't DbSets (like _context.SaveChangesAsync, though that's not in queries).
            
            # DbSets in this project: Auctions, Users, Categories, ChatRooms, Messages, Bids, Reports, Appeals, SuspiciousActivities, Notifications...
            
            # Regex: `_context\.([A-Z][a-zA-Z]+)(?=\s*\n?\s*\.)` 
            # If it matches, we can replace it with `_context.\1.AsNoTracking()`
            
            def replacer(match):
                db_set = match.group(1)
                if db_set in ['Database', 'ChangeTracker', 'Model', 'SaveChanges', 'SaveChangesAsync']:
                    return match.group(0)
                return f"_context.{db_set}.AsNoTracking()"
                
            new_content = re.sub(r'_context\.([A-Z][a-zA-Z]+)(?=\s*\n?\s*\.)', replacer, content)
            
            if new_content != content:
                with open(filepath, 'w', encoding='utf-8') as f:
                    f.write(new_content)
                print(f"Added AsNoTracking to {filename}")

