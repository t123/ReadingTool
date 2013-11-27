<?php

use Illuminate\Database\Migrations\Migration;

class UpdateLanguageSettingsFk extends Migration {

	/**
	 * Run the migrations.
	 *
	 * @return void
	 */
	public function up()
	{
            DB::statement  ('alter table languages modify settings text');
            
            Schema::table('languages', function($table) {
                $table->foreign('user_id')->references('id')->on('users');
            });
	}

	/**
	 * Reverse the migrations.
	 *
	 * @return void
	 */
	public function down()
	{
            DB::statement('alter table languages modify settings varchar(255)');
	}

}